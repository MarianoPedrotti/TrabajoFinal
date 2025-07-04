<script setup>
import { ref, onMounted, watch, computed } from 'vue'
import { useUsuarioStore } from '../store/usuarioStore'
import { useTransaccionStore } from '../store/transaccion'
import { useRouter } from 'vue-router'

const router = useRouter()


// Stores
const usuarioStore = useUsuarioStore()
const transaccionStore = useTransaccionStore()

// Estado
const monedas = ref([])
const monedaSeleccionada = ref(null)
const cantidad = ref(0)
const tipoOperacion = ref('purchase')
const cotizacion = ref(0)

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

// Cargar monedas
onMounted(async () => {
  try {
    const res = await fetch('https://localhost:7149/monedas')
    monedas.value = await res.json()
    monedaSeleccionada.value = monedas.value[0] || null
  } catch (error) {
    console.error('Error al cargar monedas:', error)
  }
})

// Cotización al cambiar
watch([monedaSeleccionada, tipoOperacion], async () => {
  if (monedaSeleccionada.value) {
    const abrev = monedaSeleccionada.value.abreviatura.toLowerCase()
    try {
      const res = await fetch(`https://criptoya.com/api/satoshitango/${abrev}/ars`)
      const data = await res.json()

      // Validar que venga bien
      const cotiza = data.totalAsk || data.totalBid || 0

      if (cotiza > 0 && cotiza < 10_000_000) {
        cotizacion.value = cotiza
      } else {
        cotizacion.value = 0
        alert('Cotización inválida o no disponible')
      }
    } catch (error) {
      console.error('Error al obtener cotización desde CriptoYa:', error)
      cotizacion.value = 0
    }
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
      <p><strong>Cotización actual:</strong> ${{ cotizacion.toFixed(2) }}</p>
      <p><strong>Total estimado:</strong> ${{ totalEstimado.toFixed(2) }}</p>
      <p><strong>Saldo en pesos:</strong> ${{ usuarioStore.usuario.saldoPesos.toFixed(2) }}</p>
      <p>
        <strong>{{ monedaSeleccionada?.abreviatura.toUpperCase() }} disponible:</strong>
        {{ usuarioStore.usuario[monedaSeleccionada?.abreviatura.toLowerCase()] || 0 }}
      </p>
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
