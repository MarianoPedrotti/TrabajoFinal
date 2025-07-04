import{createRouter, createWebHistory} from 'vue-router'
import Home from '../view/Home.vue'
import CompraVenta from '../view/CompraVenta.vue'
import Historial from '../view/Historial.vue'
const router = createRouter({
    history: createWebHistory(),
    routes:[
    {
        path: '/',
        name: 'Home',
        component: Home
    },
      {
        path: '/CompraVenta',
        name: 'CompraVenta',
        component: CompraVenta
    },
      {
        path: '/Historial',
        name: 'Historial',
        component: Historial
    }



    ]
})
export default router