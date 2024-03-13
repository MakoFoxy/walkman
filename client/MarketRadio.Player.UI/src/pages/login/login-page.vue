<template>
    <div style="margin-top: 25vh;">
      <v-container>
        <v-row style="margin-bottom: 10%;">
            <img class="center" src="../../images/logo.png" style="width: 80%; max-width: 510px;">
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
                    <v-text-field v-model="email" light="light" prepend-icon="email" label="Логин" type="email"></v-text-field>
                    <v-text-field v-model="password" light="light" prepend-icon="lock" label="Пароль" type="password"></v-text-field>

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
        v-model="snackbar.show"
        timeout="5000"
      >
        {{ snackbar.text }}

        <template v-slot:action="{ attrs }">
          <v-btn
            :color="snackbar.color"
            text
            v-bind="attrs"
            @click="snackbar.show = false"
          >
            Закрыть
          </v-btn>
        </template>
      </v-snackbar>
    </div>
</template>

<script lang="ts">
import Vue from 'vue';
import Component from 'vue-class-component';
import { mapGetters } from 'vuex';
import ObjectModel from '@/shared/models/ObjectModel';
import ObjectSelector from '../../components/object-selector/object-selector.vue';

@Component({
  computed: mapGetters(['object']),
})
export default class LoginPage extends Vue {
  private object?: ObjectModel | null

  private email = ''

  private password = ''

  private snackbar = {
    show: false,
    text: '',
    color: '',
  };

  mounted() {
    this.$nextTick(async () => {
      if (this.$route.query.force === 'true') {
        return;
      }

      if (this.object != null && this.object.id != null) {
        await this.$router.push({ name: 'main' });
      }
    });
  }

  async auth(): Promise<void> {
    const response = await fetch('/api/auth', {
      method: 'POST',
      headers: {
        Accept: 'application/json',
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        email: this.email,
        password: this.password,
      }),
    });

    if (!response.ok) {
      if (response.status === 401) {
        this.snackbar.text = 'Неправильный логин или пароль';
        this.snackbar.color = '#ca9415';
        this.snackbar.show = true;
        return;
      }

      if (response.status === 500) {
        this.snackbar.text = 'Ошибка на сервере, попробуйте позже';
        this.snackbar.color = '#8f0918';
        this.snackbar.show = true;
        return;
      }
    }

    await this.$router.push({ name: 'object-selection' });
  }
}
</script>

<style>
.center{
  margin: 0 auto;
}
</style>
