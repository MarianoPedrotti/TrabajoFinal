// src/store/usuarioStore.js
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useStorage } from '@vueuse/core'

export const useUsuarioStore = defineStore('usuarioStore', () => {
  const usuario = useStorage('usuario', {
    email: '',
    password: '',
    saldoPesos: 0,
    btc: 0,
    eth: 0,
    usds: 0
  })

  const logueado = ref(false)

  const registrarUsuario = (nombre, email, password) => {
    usuario.value = {
      email,
      password,
      saldoPesos: 100000,
      btc: 0,
      eth: 0,
      usds: 0
    }
    logueado.value = true
  }

  const iniciarSesion = (email, password) => {
    if (usuario.value.email === email && usuario.value.password === password) {
      logueado.value = true
    } else {
      throw new Error('Credenciales incorrectas')
    }
  }

  const cerrarSesion = () => {
    logueado.value = false
    localStorage.removeItem('sesion')
  }

  const guardarEnLocalStorage = () => {
    localStorage.setItem('usuario', JSON.stringify(usuario.value))
  }

  const updateSaldo = (moneda, tipo, cantidad, montoTotal) => {
    const cripto = moneda.toLowerCase() // 👈 usamos minúsculas

    if (!usuario.value.hasOwnProperty(cripto)) {
      usuario.value[cripto] = 0
    }

    if (tipo === 'purchase') {
      usuario.value.saldoPesos -= montoTotal
      usuario.value[cripto] += cantidad
    } else {
      usuario.value.saldoPesos += montoTotal
      usuario.value[cripto] -= cantidad
    }
  }

  return {
    usuario,
    logueado,
    registrarUsuario,
    iniciarSesion,
    cerrarSesion,
    guardarEnLocalStorage,
    updateSaldo
  }
})
