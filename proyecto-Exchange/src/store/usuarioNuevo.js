import { defineStore } from 'pinia'
import { ref } from 'vue'
export const useUsuarioStore = defineStore('usuario', () => {
   const users = ref({ 
      email: '',
      password: '',
      saldoPesos: 1000000,
      BTC: 0,
      ETH: 0,
      USDS: 0
    })
    const logueado = ref(false)
    const addUser = (user) => users.value.push(user);
    return{
        users,
        logueado,
        addUser,
    }
    const iniciarSesion = (email, password) => {
        const usuariosGuardados = JSON.parse(localStorage.getItem('usuarios')) || []
        const usuario = usuariosGuardados.find(u => u.email === email && u.password === password)
        if (!usuario) throw new Error('Email o contraseña incorrectos')
        users.value = { ...usuario }
        logueado.value = true
    }
})







