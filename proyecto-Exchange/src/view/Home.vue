<script setup>
import { reactive } from 'vue'
import { useRouter } from 'vue-router'
import { useUsuarioStore } from '../store/usuarioStore'
import { storeToRefs } from 'pinia'
import HaderComponet from '../components/HaderComponet.vue'

const router = useRouter()
const usuarioStore = useUsuarioStore()
const { logueado, usuario } = storeToRefs(usuarioStore)
const { registrarUsuario, iniciarSesion, cerrarSesion } = usuarioStore

const registro = reactive({
  nombre: '',
  email: '',
  password: ''
})

const login = reactive({
  email: '',
  password: ''
})

async function registrarNuevoUsuario() {
  try {
    await registrarUsuario(
      registro.nombre,
      registro.email,
      registro.password
    )

    alert('Usuario registrado con éxito')
    router.push('/CompraVenta')
  } catch (err) {
    alert(err.message)
  }
}

async function iniciarSesionUsuario() {
  try {
    await iniciarSesion(login.email, login.password)
    console.log('Usuario logueado:', usuario.value)
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
  <h1>Arium X</h1>.


  <div v-if="logueado">
    <h2>¡Hola, {{ usuario.nombre }}! Ya estás logueado ✅</h2>
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
/* ===== RESET BÁSICO ===== */
* {
  box-sizing: border-box;
}

/* ===== CONTENEDOR GENERAL ===== */
h1 {
  text-align: center;
  margin: 2rem 0 1rem;
  color: #0f172a;
}

/* ===== ESTADO LOGUEADO ===== */
div[v-if] {
  text-align: center;
}

button {
  background-color: #0f172a;
  color: white;
  border: none;
  padding: 0.6rem 1.4rem;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 500;
  transition: all 0.2s ease;
}

button:hover {
  background-color: #1e293b;
  transform: translateY(-1px);
}

/* ===== FORMULARIOS ===== */
#registro-usuario,
#iniciar-sesion {
  display: flex;
  justify-content: center;
}

form {
  background: white;
  width: 100%;
  max-width: 420px;
  padding: 2rem;
  border-radius: 14px;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.08);
}

form h1 {
  text-align: center;
  margin-bottom: 1.5rem;
  color: #0f172a;
}

/* ===== LABELS ===== */
label {
  display: block;
  margin-bottom: 0.3rem;
  font-size: 0.9rem;
  color: #475569;
}

/* ===== INPUTS ===== */
input {
  width: 100%;
  padding: 0.6rem 0.75rem;
  margin-bottom: 1.2rem;
  border-radius: 8px;
  border: 1px solid #cbd5e1;
  font-size: 0.95rem;
  transition: border-color 0.2s, box-shadow 0.2s;
}

input:focus {
  outline: none;
  border-color: #0f172a;
  box-shadow: 0 0 0 2px rgba(15, 23, 42, 0.15);
}

/* ===== BOTÓN FORM ===== */
form button {
  width: 100%;
  margin-top: 0.5rem;
  font-size: 1rem;
}

/* ===== LINKS ===== */
a button {
  margin: 0.5rem;
}
:global(body) {
  margin: 0;
  min-height: 100vh;
  background-color: #f1f5f9; /* gris claro profesional */
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
}

</style>