<script setup>
import { reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useUsuarioStore } from '../store/usuarioNuevo';
import { storeToRefs } from 'pinia';
import HaderComponet from '../components/HaderComponet.vue';


const router = useRouter();
const usuarioStore = useUsuarioStore();
const { usuarioLogeado} = storeToRefs(usuarioStore);
async function registrarNuevoUsuario() {
  try {
    useUsuarioStore.adduser(
      user.email,
      user.password
    )
    useUsuarioStore.logueado = true

    alert('Usuario registrado con éxito')

    // Redirigir al login
    await iniciarSesion(user.email, user.password)
    router.push('/CompraVenta')

    // Limpiar
    
    user.email = ''
    user.password = ''
  } catch (err) {
    alert(err.message)
  }

  const user = ref({
    email: '',
    password: ''
  });
}
async function iniciarSesionUsuario() {
  
const login = ref({
  email: '',
  password: ''
});}
</script>
<template>
  <HaderComponet />

  <div v-if="usuarioLogeado">
    <h2>¡Hola Ya estás logueado ✅</h2>
    <button @click="cerrarSesionUsuario">Cerrar sesión</button>
    <br /><br />
    <router-link to="/CompraVenta"><button>Compra | Vender</button></router-link>
    <router-link to="/Historial"><button>Ver Historial</button></router-link>
  </div>

  <div v-else>
    <div id="registro-usuario">
      <form @submit.prevent="registrarNuevoUsuario">
        <h1>Registro de Usuario</h1>

        <label for="email">Email:</label>
        <input type="email" id="email" v-model="user.email" required />

        <label for="password">Contraseña:</label>
        <input type="password" id="password" v-model="user.password" required />

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