import { defineStore } from 'pinia'
import { ref, onMounted } from 'vue'

export const useMonedaStore = defineStore('moneda', () => {
  const monedas = ref([])

  async function cargarMonedas() {
    try {
      const res = await fetch('https://localhost:7194/monedas')
      monedas.value = await res.json()
      console.log(monedas.value)
    } catch (error) {
      console.error('Error cargando monedas:', error)
    }
  }

  return {
    monedas,
    cargarMonedas
  }
})
