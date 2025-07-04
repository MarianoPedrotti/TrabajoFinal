<script setup>
// Si necesitás importar la imagen desde assets
import logo from '@/assets/Logo.png' // Cambiá el path según tu estructura
</script>

<template>
  <header class="header">
    <img :src="logo" alt="Logo AriumX" class="logo" />
    <h1>AriumX | Exchange Digital</h1>
  </header>
</template>

<style scoped>
.header {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem 2rem;
  background-color: #0f172a; /* Azul oscuro */
  color: white;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
}

.logo {
  height: 50px;
  width: auto;
}
</style>
