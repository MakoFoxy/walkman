<template>
  <v-app>
    <v-app-bar v-if="isAuthenticated" app flat>
      <v-app-bar-nav-icon @click="menuIsOpen = true"></v-app-bar-nav-icon>
      <div class="d-flex align-center">
        <v-toolbar-title>Загрузчик сборок</v-toolbar-title>
      </div>
    </v-app-bar>
    <v-main>
      <v-navigation-drawer
          v-model="menuIsOpen"
          app
          temporary
      >
        <v-list
            nav
            dense
        >
          <v-list-item-group>
            <v-list-item to="/tasks-page">
              <v-list-item-title>
                Задания
              </v-list-item-title>
            </v-list-item>
            <v-list-item to="/selections-page">
              <v-list-item-title>
                Подборки
              </v-list-item-title>
            </v-list-item>
            <v-list-item to="/tracks-page">
              <v-list-item-title>
                Треки
              </v-list-item-title>
            </v-list-item>
            <v-list-item to="/genres-page">
              <v-list-item-title>
                Жанры
              </v-list-item-title>
            </v-list-item>
            <v-list-item to="/create-selections-from-folder-page">
              <v-list-item-title>
                Создание подборок из папки
              </v-list-item-title>
            </v-list-item>
          </v-list-item-group>
        </v-list>
      </v-navigation-drawer>
      <loading v-if="loading" />
      <router-view v-else/>
    </v-main>
  </v-app>
</template>

<script lang="ts">
import {defineComponent, ref, toRefs} from '@vue/composition-api';
import {Container} from '@/Container';
import {Store} from '@/store';
import Loading from '@/components/Loading.vue';
import {useRouter} from '@/router/router-composition-api';

export default defineComponent({
  name: 'App',
  components: {Loading},
  setup: () => {
    Container.setup();
    const router = useRouter();

    const store = Container.resolve<Store>(nameof(Store));
    const loading = ref(true);
    const menuIsOpen = ref(false);

    const { isAuthenticated } = toRefs(store.state.reactive);

    store.initState()
    .then(() => {
      loading.value = false;
      if (!isAuthenticated) {
        router.push('/login-page');
      }
    });

    return {
      loading,
      menuIsOpen,
      isAuthenticated: isAuthenticated,
    };
  }
});
</script>

<style lang="stylus" scoped>

</style>
