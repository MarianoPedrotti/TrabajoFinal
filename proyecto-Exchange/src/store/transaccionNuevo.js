// src/store/transaccionStore.ts
import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useTransaccionStore = defineStore('transaccion', () => {
  const transaccion = ref({
    monedaId: null,
    cantidad: 0,
    cotizacion: 0,
    fecha: '',
    tipo: 'purchase', // 'purchase' o 'sale'
    usuarioId: null,
    usuario:'',
    moneda:'',
  })

  return {
    transaccion
  }
})
