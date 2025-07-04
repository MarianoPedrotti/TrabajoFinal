<script setup>
import { ref, onMounted, computed } from 'vue'

const transacciones = ref([])
const filtroTipo = ref('todos')
const seleccionadas = ref(new Set())

const cargarTransacciones = async () => {
  try {
    const res = await fetch('https://localhost:7149/transactions')
    const data = await res.json()
    transacciones.value = data
  } catch (error) {
    console.error('Error cargando transacciones:', error)
  }
}

const transaccionesFiltradas = computed(() => {
  if (filtroTipo.value === 'todos') return transacciones.value
  return transacciones.value.filter(t => t.action === filtroTipo.value)
})

const formatearFecha = iso => {
  const fecha = new Date(iso)
  return isNaN(fecha) ? 'Fecha inválida' : fecha.toLocaleString('es-AR')
}

const toggleSeleccion = (id) => {
  if (seleccionadas.value.has(id)) {
    seleccionadas.value.delete(id)
  } else {
    seleccionadas.value.add(id)
  }
}

const eliminarSeleccionadas = async () => {
  const confirmacion = confirm('¿Seguro que querés eliminar las transacciones seleccionadas?')
  if (!confirmacion) return

  for (const id of seleccionadas.value) {
    try {
      await fetch(`https://localhost:7149/transactions/${id}`, { method: 'DELETE' })
    } catch (error) {
      console.error(`Error al eliminar transacción ${id}:`, error)
    }
  }

  await cargarTransacciones()
  seleccionadas.value.clear()
}

onMounted(cargarTransacciones)
</script>

<template>
  <div class="historial">
    <h2>Historial de Transacciones</h2>

    <div class="filtro">
      <label>Filtrar por tipo:</label>
      <select v-model="filtroTipo">
        <option value="todos">Todos</option>
        <option value="purchase">Compra</option>
        <option value="sale">Venta</option>
      </select>
    </div>

    <table border="1" cellpadding="8" cellspacing="0">
      <thead>
        <tr>
          <th>Seleccionar</th>
          <th>Fecha</th>
          <th>Moneda</th>
          <th>Cantidad</th>
          <th>Cotización</th>
          <th>Total</th>
          <th>Tipo</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="t in transaccionesFiltradas" :key="t.id">
          <td>
            <input type="checkbox" :value="t.id" @change="toggleSeleccion(t.id)" />
          </td>
          <td>{{ formatearFecha(t.datetime) }}</td>
          <td>{{ t.crypto_code.toUpperCase() }}</td>
          <td>{{ t.crypto_amount }}</td>
          <td>${{ t.money / t.crypto_amount }}</td>
          <td>${{ t.money.toFixed(2) }}</td>
          <td>{{ t.action === 'purchase' ? 'Compra' : 'Venta' }}</td>
        </tr>
      </tbody>
    </table>

    <button @click="eliminarSeleccionadas" :disabled="seleccionadas.size === 0">
      Eliminar seleccionadas
    </button>
  </div>
</template>

<style scoped>
.historial {
  max-width: 900px;
  margin: auto;
  padding: 2rem;
}

.filtro {
  margin-bottom: 1rem;
}

table {
  width: 100%;
  margin-bottom: 1rem;
}
</style>
