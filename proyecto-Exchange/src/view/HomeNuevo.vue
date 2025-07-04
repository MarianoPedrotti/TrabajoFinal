<script setup>
import { reactive } from 'vue'
import { useRouter } from 'vue-router' // ⬅️ Importar router
import { useUsuarioStore } from '../store/usuarioStore'
import { storeToRefs } from 'pinia'
import HaderComponet from '../components/HaderComponet.vue'

const router = useRouter() // ⬅️ Instanciar el router

const usuarioStore = useUsuarioStore()
const { usuarioLogeado, nombreUsuario } = storeToRefs(usuarioStore)
const { registrarUsuario, iniciarSesion, cerrarSesion } = usuarioStore

const registro = reactive({
  nombre: '',
  email: '',
  password: '',
  pesosArg: 1000000
})

const login = reactive({
  email: '',
  password: ''
})

async function registrarNuevoUsuario() {
  await registrarUsuario(
    registro.nombre,
    registro.email,
    registro.password,
    registro.pesosArg
  )

  registro.nombre = ''
  registro.email = ''
  registro.password = ''
}

async function iniciarSesionUsuario() {
  try {
    await iniciarSesion(login.email, login.password)

    // 🔁 Redirige solo si el login fue exitoso
    router.push('/CompraVenta')
  } catch (error) {
    alert('Email o contraseña incorrectos')
  }

  login.email = ''
  login.password = ''
}
</script>

<template>
  <HaderComponet />

  <div v-if="usuarioLogeado">
    <h2>¡Hola, {{ nombreUsuario }}! Ya estás logueado ✅</h2>
    <button @click="cerrarSesion">Cerrar sesión</button>
    <br /><br />
    <router-link to="/CompraVenta"><button>Compra | Vender</button></router-link>
    <router-link to="/Historial"><button>Ver Historial</button></router-link>
  </div>

  <div v-else>
    <div id="registro-usuario">
      <form @submit.prevent="registrarNuevoUsuario">
        <h1>Registro de Usuario</h1>
        <label for="nombre">Nombre:</label>
        <input type="text" id="nombre" v-model="registro.nombre" required />

        <label for="email">Email:</label>
        <input type="email" id="email" v-model="registro.email" required />

        <label for="password">Contraseña:</label>
        <input type="password" id="password" v-model="registro.password" required />

        <button type="submit">Registrar</button>
      </form>
    </div>

    <div id="iniciar-sesion" style="margin-top: 2rem">
      <form @submit.prevent="iniciarSesionUsuario">
        <h1>Iniciar Sesión</h1>
        <label for="email-login">Email:</label>
        <input type="email" id="email-login" v-model="login.email" required />

        <label for="password-login">Contraseña:</label>
        <input type="password" id="password-login" v-model="login.password" required />

        <button type="submit">Iniciar Sesión</button>
      </form>
    </div>
  </div>
</template>

<style scoped>
form {
  margin-bottom: 2rem;
  padding: 1rem;
  border: 1px solid #ccc;
  max-width: 400px;
}

label {
  display: block;
  margin: 0.5rem 0 0.2rem;
}

input {
  width: 100%;
  padding: 0.4rem;
  margin-bottom: 1rem;
}

button {
  padding: 0.5rem 1rem;
}
</style>
