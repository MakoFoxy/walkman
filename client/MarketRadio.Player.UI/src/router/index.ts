import Vue from 'vue';
import VueRouter from 'vue-router';
import MainPage from '@/pages/main/main-page.vue';
import LoginPage from '@/pages/login/login-page.vue';
import ObjectSelection from '@/pages/object-selection/object-selection.vue';
import SettingsPage from '@/pages/settings-page/settings-page.vue';

Vue.use(VueRouter);

const routes = [
  {
    path: '/',
    name: 'login',
    component: LoginPage,
  },
  {
    path: '/main',
    name: 'main',
    component: MainPage,
  },
  {
    path: '/object-selection',
    name: 'object-selection',
    component: ObjectSelection,
  },
  {
    path: '/settings-page',
    name: 'settings-page',
    component: SettingsPage,
  },
];

const router = new VueRouter({
  base: process.env.BASE_URL,
  routes,
  // mode: 'history',
});

export default router;
