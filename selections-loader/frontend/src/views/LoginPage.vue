<template>
  <div style="margin-top: 25vh;">
    <v-container>
      <v-row style="margin-bottom: 10%;">
        <img class="center" src="../images/logo.png" style="width: 80%; max-width: 510px;">
      </v-row>
      <v-row style="margin-bottom: 5%;">
        <h3 class="center">Радиовещание в Торговых центрах РК</h3>
      </v-row>
      <v-row>
        <h3 class="center">Вход</h3>
      </v-row>
      <v-row justify="center" style="margin-bottom: 10%; margin-left: 10px; margin-right: 10px;">
        <v-layout align-center="align-center" justify-center="justify-center">
          <v-flex class="login-form text-xs-center">
            <v-card light="light">
              <v-card-text>
                <v-form>
                  <v-text-field v-model="reactiveData.loginForm.login" light="light" prepend-icon="email" label="Логин" type="email"></v-text-field>
                  <v-text-field v-model="reactiveData.loginForm.password" light="light" prepend-icon="lock" label="Пароль" type="password"></v-text-field>

                  <v-row justify="center">
                    <v-btn class="text-center" @click="auth">Войти</v-btn>
                  </v-row>
                </v-form>
              </v-card-text>
            </v-card>
          </v-flex>
        </v-layout>
      </v-row>
    </v-container>
    <v-snackbar
        v-model="reactiveData.snackbar.show"
        timeout="5000"
    >
      {{ reactiveData.snackbar.text }}

      <template v-slot:action="{ attrs }">
        <v-btn
            :color="reactiveData.snackbar.color"
            text
            v-bind="attrs"
            @click="reactiveData.snackbar.show = false"
        >
          Закрыть
        </v-btn>
      </template>
    </v-snackbar>
  </div>
</template>

<script lang="ts">
import {defineComponent, nextTick, onMounted, reactive} from '@vue/composition-api';
import {Container} from '@/Container';
import {Store} from '@/store';
import {useRouter} from '@/router/router-composition-api';

export default defineComponent({
  name: 'LoginPage',
  setup: () => {
    const store = Container.resolve<Store>(nameof(Store));
    const router = useRouter();

    onMounted(() => {
      nextTick(() => {
        if (store.state.reactive.isAuthenticated) {
          router.push('/tasks-page');
        }
      });
    });

    const reactiveData = reactive({
      loginForm: {
        login: '',
        password: '',
      },
      snackbar: {
        show: false,
        color: '',
        text: '',
      },
    });

    const auth = async () => {
      const response = await fetch('/api/auth', {
        method: 'POST',
        headers: {
          Accept: 'application/json',
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          email: reactiveData.loginForm.login,
          password: reactiveData.loginForm.password,
        }),
      });

      if (response.ok) {
        store.state.reactive.isAuthenticated = true;
        await store.initState();
        await router.push('/tasks-page');
      }
    };

    return {
      reactiveData,
      auth,
    };
  },
});
</script>

<style scoped>

.center{
  margin: 0 auto;
}
</style>
