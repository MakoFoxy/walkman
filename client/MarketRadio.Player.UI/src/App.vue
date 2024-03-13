<template>
  <v-app>
    <v-main>
      <router-view/>
    </v-main>
  </v-app>
</template>

<script lang="ts">
import Vue from 'vue';
import { mapGetters } from 'vuex';
import Component from 'vue-class-component';

    @Component({
      computed: mapGetters(['playlistUpdating']),
    })
export default class App extends Vue {
  name = 'App'

  playlistUpdating!: boolean
}
</script>

<style lang="sass">
  @import '../node_modules/typeface-roboto/index.css'
</style>
