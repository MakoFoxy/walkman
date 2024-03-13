import Vue from 'vue';
import VueRouter, { RouteConfig } from 'vue-router';

Vue.use(VueRouter);

const routes: Array<RouteConfig> = [
  {
    path: '/login-page',
    name: 'LoginPage',
    component: () => import(/* webpackChunkName: "login-page" */ '../views/LoginPage.vue'),
  },
  {
    path: '/tracks-page',
    name: 'TracksPage',
    component: () => import(/* webpackChunkName: "tracks-page" */ '../views/TracksPage.vue'),
  },
  {
    path: '/tasks-page',
    name: 'TasksPage',
    component: () => import(/* webpackChunkName: "tasks-page" */ '../views/TasksPage.vue'),
  },
  {
    path: '/genres-page',
    name: 'GenresPage',
    component: () => import(/* webpackChunkName: "genres-page" */ '../views/GenresPage.vue'),
  },
  {
    path: '/create-selections-from-folder-page',
    name: 'CreateSelectionsFromFolderPage',
    component: () => import(/* webpackChunkName: "create-selections-from-folder-page" */ '../views/CreateSelectionsFromFolderPage.vue'),
  },
  {
    path: '/selections-page',
    name: 'SelectionsPage',
    component: () => import(/* webpackChunkName: "selections-page" */ '../views/SelectionsPage.vue'),
  },
  {
    path: '*',
    component: () => import(/* webpackChunkName: "login-page" */ '../views/LoginPage.vue'),
  }
];

const router = new VueRouter({
  mode: 'history',
  base: process.env.BASE_URL,
  routes,
});

export default router;
