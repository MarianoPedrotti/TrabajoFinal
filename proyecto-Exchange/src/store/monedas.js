import { defineStore } from 'pinia'
import { ref } from 'vue'

const API_BASE = import.meta.env.VITE_API_URL || 'https://localhost:7149'

export const useMonedaStore = defineStore('moneda', () => {
  const monedas = ref([])

  async function cargarMonedas() {
    try {
      const res = await fetch(`${API_BASE}/monedas`)
      console.log('Fetch /monedas status:', res.status, 'ok:', res.ok)

      const text = await res.text()
      console.log('Respuesta /monedas (texto):', text)

      if (!res.ok) {
        console.error('Respuesta no OK al cargar monedas:', res.status, text)
        return false
      }

      // Intento parsear la respuesta
      let parsed
      try {
        parsed = text ? JSON.parse(text) : null
      } catch (parseErr) {
        console.error('Error parseando JSON de /monedas:', parseErr, 'raw:', text)
        return false
      }

      // Validar estructura esperada (array)
      if (!Array.isArray(parsed)) {
        console.error('La respuesta de /monedas no es un array:', parsed)
        return false
      }

      monedas.value = parsed
      console.log('Monedas cargadas:', monedas.value)
      return true
    } catch (error) {
      console.error('Error cargando monedas:', error)
      return false
    }
  }

  return {
    monedas,
    cargarMonedas
  }
})
