<script setup>
import { ref, onMounted, watch, computed } from 'vue'
import { useUsuarioStore } from '../store/usuarioStore'
import { useTransaccionStore } from '../store/transaccion'
import { useMonedaStore } from '../store/monedas'
import { useRouter } from 'vue-router'

const router = useRouter()


// Stores
const usuarioStore = useUsuarioStore()
const transaccionStore = useTransaccionStore()
const monedaStore = useMonedaStore()

// Estado
const monedas = monedaStore.monedas
const monedaSeleccionada = ref(null)
const cantidad = ref(0)
const tipoOperacion = ref('purchase')
const cotizacion = ref(0)

// Diagnóstico de cotizaciones
const diag = ref({ lastAbrev: null, lastTipo: null, rawData: null, chosenPrice: null, notes: [] })

// Total en pesos
const totalEstimado = computed(() => cotizacion.value * cantidad.value)

const realizadaTransaccion = async () => {
  if (!monedaSeleccionada.value) {
    alert('Faltan datos para la transacción.')
    return
  }

  const montoTotal = totalEstimado.value
  const monedaAbrev = monedaSeleccionada.value.abreviatura.toLowerCase()

  // Verificar saldos
  if (tipoOperacion.value === 'purchase' && usuarioStore.usuario.saldoPesos < montoTotal) {
    alert('Saldo insuficiente para realizar la compra.')
    return
  }

  if (tipoOperacion.value === 'sale' && usuarioStore.usuario[monedaAbrev] < cantidad.value) {
    alert(`No tenés suficientes ${monedaAbrev.toUpperCase()} para vender.`)
    return
  }

  const offset = new Date().getTimezoneOffset() * 60000
  const transaccion = {
    monedaId: monedaSeleccionada.value.id,
    cantidad: cantidad.value,
    cotizacion: cotizacion.value,
    montoTotal,
    fecha: new Date(Date.now() - offset).toISOString(),
    tipo: tipoOperacion.value
  }

  try {
    const response = await fetch('https://localhost:7149/transactions', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(transaccion)
    })

    if (!response.ok) {
      const texto = await response.text()
      console.error('Error del servidor:', texto)
      alert('Error al realizar la transacción: ' + texto)
      return
    }

    // ✅ Actualizar saldos locales
    usuarioStore.updateSaldo(monedaAbrev.toLowerCase(), tipoOperacion.value, cantidad.value, montoTotal)

    usuarioStore.guardarEnLocalStorage?.() // por compatibilidad

    alert('Transacción exitosa.')
    cantidad.value = 0
  } catch (error) {
    console.error('Error al enviar la transacción:', error)
    alert('Error al enviar la transacción.')
  }
}

// Cargar monedas desde la store
onMounted(async () => {
  try {
    const ok = await monedaStore.cargarMonedas()
    console.log('cargarMonedas ok:', ok, 'monedaStore.monedas raw:', monedaStore.monedas)

    // Detecto si monedaStore.monedas es un array directo o un ref que contiene un array
    const lista = Array.isArray(monedaStore.monedas)
      ? monedaStore.monedas
      : (Array.isArray(monedaStore.monedas?.value) ? monedaStore.monedas.value : [])

    const first = lista[0] ?? null
    if (ok && first) {
      monedaSeleccionada.value = first
      console.log('monedaSeleccionada seteada:', monedaSeleccionada.value)
    } else {
      monedaSeleccionada.value = null
      console.error('No se pudieron cargar las monedas desde la API o la lista está vacía', { ok, first, monedas: lista })
    }
  } catch (error) {
    console.error('Error al cargar monedas:', error)
  }
})

// Cotización al cambiar
watch([monedaSeleccionada, tipoOperacion], async () => {
  if (monedaSeleccionada.value && monedaSeleccionada.value.abreviatura) {
    const abrev = monedaSeleccionada.value.abreviatura.toLowerCase()
    try {
      const res = await fetch(`https://criptoya.com/api/satoshitango/${abrev}/ars`)
      const data = await res.json()

      // Log para diagnóstico
      console.log('Cotización criptoya:', { abrev, tipo: tipoOperacion.value, data })

      // Guardar raw en diag
      diag.value.lastAbrev = abrev
      diag.value.lastTipo = tipoOperacion.value
      diag.value.rawData = data
      diag.value.notes = []

      // Helpers
      const parseFlexibleNumber = (v) => {
        if (typeof v === 'number') return v
        if (!v && v !== 0) return null
        let s = String(v).trim()
        // Si tiene ambos separadores, quitar comas (miles)
        if (s.includes('.') && s.includes(',')) s = s.replace(/,/g, '')
        else if (s.includes(',') && !s.includes('.')) s = s.replace(',', '.')
        // Quitar cualquier caracter que no sea dígito, punto, signo o exponente
        s = s.replace(/[^0-9+\-\.eE]/g, '')
        const n = Number(s)
        return Number.isFinite(n) ? n : null
      }

      const attemptScales = (n) => {
        // si n está en rango plausible en ARS, devolverlo. Aumentamos límite superior a 1e9
        if (n > 0 && n < 1_000_000_000) return n
        // probar divisores si es muy grande (pero preferir no reducir si posible)
        const divisores = [10, 100, 1000, 10000, 100000, 1e6, 1e8]
        for (const d of divisores) {
          const v = n / d
          if (v > 0 && v < 1_000_000_000) {
            diag.value.notes.push(`Scaled by /${d}: ${n} -> ${v}`)
            return v
          }
        }
        // probar multiplicadores si muy pequeño positivo
        const multiplicadores = [10, 100, 1000, 1000000, 1e8]
        for (const m of multiplicadores) {
          const v = n * m
          if (v > 0 && v < 1_000_000_000) {
            diag.value.notes.push(`Scaled by *${m}: ${n} -> ${v}`)
            return v
          }
        }
        return null
      }

      const findCandidate = (obj, keys) => {
        for (const k of keys) {
          if (!obj) continue
          // permitir campos anidados simples
          const raw = obj[k]
          const parsed = parseFlexibleNumber(raw)
          if (parsed !== null) {
            const normalized = attemptScales(parsed)
            if (normalized !== null) {
              diag.value.notes.push(`Field ${k} -> raw=${raw} parsed=${parsed} norm=${normalized}`)
              return normalized
            } else {
              diag.value.notes.push(`Field ${k} -> raw=${raw} parsed=${parsed} (fuera de rango)`)
            }
          }
        }
        return null
      }

      // Priorizar campos más 'standard' (ask/bid) antes de los agregados totalAsk/totalBid
      const candidatesOrder = [
        ['ask', 'totalAsk', 'sell'],
        ['bid', 'totalBid', 'buy'],
        ['last', 'price']
      ]

      // Guardar campo elegido para transparencia
      let chosenField = null

      let precio = null

      if (tipoOperacion.value === 'purchase') {
        for (const keys of candidatesOrder) {
          const found = findCandidate(data, keys)
          if (found) {
            precio = found
            chosenField = keys.find(k => Object.prototype.hasOwnProperty.call(data, k)) || null
            break
          }
        }
      } else {
        // venta: invertir prioridad
        for (const keys of [candidatesOrder[1], candidatesOrder[0], candidatesOrder[2]]) {
          const found = findCandidate(data, keys)
          if (found) {
            precio = found
            chosenField = keys.find(k => Object.prototype.hasOwnProperty.call(data, k)) || null
            break
          }
        }
      }

      // último recurso: revisar todo el objeto
      if (!precio) {
        for (const k in data) {
          const parsed = parseFlexibleNumber(data[k])
          if (parsed !== null) {
            const norm = attemptScales(parsed)
            if (norm !== null) {
              diag.value.notes.push(`Field ${k} (fallback) -> raw=${data[k]} parsed=${parsed} norm=${norm}`)
              precio = norm
              chosenField = k
              break
            }
          }
        }
      }

      if (precio && precio > 0 && precio < 10_000_000) {
        cotizacion.value = precio
        diag.value.chosenPrice = precio
        diag.value.sourceField = chosenField
        console.log('Precio elegido:', precio, 'campo:', chosenField)
      } else {
        cotizacion.value = 0
        diag.value.chosenPrice = null
        diag.value.sourceField = chosenField
        console.warn('Cotización inválida o no disponible para', abrev, 'data:', data)
      }
    } catch (error) {
      console.error('Error al obtener cotización desde CriptoYa:', error)
      cotizacion.value = 0
    }
  } else {
    cotizacion.value = 0
  }
})


</script>

<template>
  <div class="compra-venta">
    <h2>Compra/Venta de Criptomonedas</h2>

    <div class="form-group">
      <label for="moneda">Moneda:</label>
      <select v-model="monedaSeleccionada" id="moneda">
        <option v-for="moneda in monedas" :key="moneda.id" :value="moneda">
          {{ moneda.nombre }}
        </option>
      </select>
    </div>

    <div class="form-group">
      <label for="cantidad">Cantidad:</label>
      <input
        type="number"
        id="cantidad"
        v-model.number="cantidad"
        min="0.01"
        step="0.01"
      />
    </div>

    <div class="form-group">
      <label for="tipo">Tipo de operación:</label>
      <select v-model="tipoOperacion" id="tipo">
        <option value="purchase">Compra</option>
        <option value="sale">Venta</option>
      </select>
    </div>

    <div class="info">
      <p><strong>Cotización actual:</strong> ${{ cotizacion > 0 ? cotizacion.toFixed(2) : 'N/D' }}</p>
      <p><strong>Total estimado:</strong> ${{ totalEstimado.toFixed(2) }}</p>
      <p><strong>Saldo en pesos:</strong> ${{ usuarioStore.usuario.saldoPesos.toFixed(2) }}</p>
      <p>
        <strong>{{ monedaSeleccionada && monedaSeleccionada.abreviatura ? monedaSeleccionada.abreviatura.toUpperCase() : '' }} disponible:</strong>
        {{ monedaSeleccionada && monedaSeleccionada.abreviatura ? (usuarioStore.usuario[monedaSeleccionada.abreviatura.toLowerCase()] || 0) : 0 }}
      </p>

      <!-- Panel diagnóstico (visible en desarrollo) -->
      <details style="margin-top:8px; font-size:13px;">
        <summary>Diagnóstico cotización</summary>
        <div style="white-space:pre-wrap; margin-top:8px">
          <strong>Última moneda:</strong> {{ diag.lastAbrev }} ({{ diag.lastTipo }})
          <br />
          <strong>Precio elegido:</strong> {{ diag.chosenPrice ?? 'N/D' }} (campo: {{ diag.sourceField ?? 'N/D' }})
          <br />
          <strong>Notas:</strong>
          <div v-if="diag.notes.length===0">- sin notas</div>
          <div v-for="(n, i) in diag.notes" :key="i">- {{ n }}</div>
          <br />
          <strong>Raw data:</strong>
          <pre>{{ JSON.stringify(diag.rawData, null, 2) }}</pre>
        </div>
      </details>
    </div>

    <button @click="realizadaTransaccion">
      Realizar {{ tipoOperacion === 'purchase' ? 'compra' : 'venta' }}
    </button>
   <button @click="router.push('/')" class="secundario">
  Volver al inicio
</button>
<button @click="router.push('/Historial')" class="secundario">
  Ver historial
</button>

  </div>
</template>

<style scoped>
.compra-venta {
  max-width: 500px;
  margin: auto;
  padding: 1rem;
}

.form-group {
  margin-bottom: 1rem;
}

input, select {
  width: 100%;
  padding: 0.5rem;
  margin-top: 0.25rem;
}
.volver-btn {
  margin-top: 1rem;
  background-color: #d35252;
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 6px;
  cursor: pointer;
}

.volver-btn:hover {
  background-color: #a80a0a;
}
.secundario {
  margin-top: 1rem;
  margin-right: 0.5rem;
  background-color: #eee;
  padding: 0.5rem 1rem;
  border: 1px solid #ccc;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
}

.secundario:hover {
  background-color: #ddd;
}

</style>
