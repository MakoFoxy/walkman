import Vue from 'vue';
import VueCompositionAPI, { reactive } from '@vue/composition-api';
import router from './index';

Vue.use(VueCompositionAPI);

const currentRoute = reactive({
  ...router.currentRoute,
});

const currentRouter = reactive({
  ...router,
  getRoutes: router.getRoutes,
  push: router.push,
});

router.beforeEach((to, from, next) => {
  Object.keys(to).forEach((key) => {
    (currentRoute as any)[key] = (to as any)[key];
  });
  next();
});

export function useRoute() {
  return currentRoute;
}

export function useRouter() {
  return currentRouter;
}
