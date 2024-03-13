<template>
  <v-app>
    <v-content transition="slide-x-transition">
      <router-view v-if="!isLoading"/>
      <v-overlay
          :opacity="1"
          color="blue"
          :value="isLoading"
      >
        <v-progress-circular
            indeterminate
            size="220">
          Загрузка...
        </v-progress-circular>
      </v-overlay>
      <error-message></error-message>
    </v-content>
</v-app>
</template>

<script>
import ErrorMessage from './components/ErrorMessage'
import { parseJwt } from './helpers/JwtService'
import {GET_PERMISSIONS, LOGOUT} from "@/store/actions.type";
import {GET_USER_OBJECTS} from "@/store/actions.type";

export default {
  name: 'app',
  components: {
    ErrorMessage
  },
  data: () => ({
    isLoading: true,
  }),
  async mounted() {
    if (this.$router.currentRoute.name === 'login') {
      this.isLoading = false;
      return;
    }
    const tokenData = parseJwt(localStorage.getItem('token'))
    if (!tokenData.tokenIsCorrect) {
      await this.$store.dispatch(LOGOUT);
      await this.$router.push('/login');
      this.isLoading = false;
      return;
    }
    await this.$store.dispatch(GET_PERMISSIONS);
    await this.$store.dispatch(GET_USER_OBJECTS);
    this.isLoading = false;
  }
}
</script>
