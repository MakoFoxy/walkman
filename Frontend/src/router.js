import Vue from 'vue'
import Router from 'vue-router'
import Login from './views/Login.vue'

Vue.use(Router)

export default new Router({
  mode: 'history',
  base: process.env.BASE_URL,
  routes: [
    {
      path: '/login',
      name: 'login',
      component: Login,
    },
    {
      path: '/object-list',
      name: 'object-list',
      component: () => import(/* webpackChunkName: "object-list" */ './views/ObjectList.vue')
    },
    {
      path: '/add-object',
      name: 'add-object',
      component: () => import(/* webpackChunkName: "add-object" */ './views/AddObject.vue')
    },
    {
      path: '/add-advert',
      name: 'add-advert',
      component: () => import(/* webpackChunkName: "add-advert" */ './views/AddAdvert.vue')
    },
    {
      path: '/home',
      name: 'home',
      component: () => import(/* webpackChunkName: "home" */ './views/Home.vue')
    },
    {
      path: '/edit-object',
      name: 'edit-object',
      component: () => import(/* webpackChunkName: "edit-object" */ './views/EditObject.vue')
    },
    {
      path: '/add-music',
      name: 'add-music',
      component: () => import(/* webpackChunkName: "add-music"*/ './views/AddMusic.vue')
    },
    {
      path: '/add-client',
      name: 'add-client',
      component: () => import(/* webpackChunkName: "add-client"*/ './views/AddClient.vue')
    },
    {
      path: '/edit-client',
      name: 'edit-client',
      component: () => import(/* webpackChunkName: "edit-client"*/ './views/EditClient.vue')
    },
    {
      path: '/advert-card',
      name: 'advert-card',
      component: () => import(/* webpackChunkName: "advert-card"*/ './views/AdvertCard.vue')
    },
    {
      path: '/add-admin',
      name: 'add-admin',
      component: () => import(/* webpackChunkName: "add-admin"*/ './views/AddAdmin.vue')
    },
    {
      path: '/edit-admin',
      name: 'edit-admin',
      component: () => import(/* webpackChunkName: "edit-admin"*/ './views/EditAdmin.vue')
    },
    {
      path: '/home-client',
      name: 'home-client',
      component: () => import(/* webpackChunkName: "home-client"*/ './views/HomeClient.vue')
    },
    {
      path: '*',
      component: Login
    }
  ]
})
