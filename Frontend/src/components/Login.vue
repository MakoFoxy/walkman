<template>
  <v-container fluid fill-height>
    <v-layout align-center justify-center>
      <v-flex xs12 sm8 md4>
        <v-card class="elevation-12">
          <v-toolbar dark color="primary">
            <v-toolbar-title>Вход в систему</v-toolbar-title>
            <v-spacer></v-spacer>
          </v-toolbar>
          <v-card-text>
            <v-form>
              <v-text-field prepend-icon="person" name="email" label="Email" type="email" v-model="email"></v-text-field>
              <v-text-field id="password" prepend-icon="lock" name="password" label="Пароль" type="password"
                v-model="password"></v-text-field>
            </v-form>
          </v-card-text>
          <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn color="primary" @click="loginButtonClick(email, password)" :loading="loading">Войти</v-btn>
          </v-card-actions>
        </v-card>
      </v-flex>
    </v-layout>
  </v-container>
</template>

<script>
import {GET_PERMISSIONS, GET_USER_OBJECTS, LOGIN, REGISTER_ERROR} from "../store/actions.type"
import {AdminAccessObject, PartnerAccessToObject} from "@/helpers/permissions";

  export default {
    name: 'login',
    data: () => ({
      email: '',
      password: '',
      loading: false
    }),
    methods: {
      async loginButtonClick(email, password) {
        this.loading = true;
        try {
          await this.$store.dispatch(LOGIN, { email, password });
          await this.$store.dispatch(GET_PERMISSIONS);
          await this.$store.dispatch(GET_USER_OBJECTS);
        }
        catch (error){
          this.$store.dispatch(REGISTER_ERROR, JSON.parse(error.message));
          return;
        }
        finally {
          this.loading = false;
        }
        if (
            this.$store.state.user.user.permissions.includes(PartnerAccessToObject) ||
            this.$store.state.user.user.permissions.includes(AdminAccessObject)
        ){
          this.$router.push('/home-client');
          return;
        }

        this.$router.push('/home');
      }
    }
  }
</script>
