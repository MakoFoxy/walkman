<template>
  <v-container>
    <full-settings />
  </v-container>
</template>

<script lang="ts">
import Vue from 'vue';
import Component from 'vue-class-component';
import FullSettings from '@/components/settings/full-settings.vue';

@Component({
  name: 'settings-page',
  components: {
    FullSettings,
  },
})
export default class SettingsPage extends Vue {
}
</script>

<style scoped>

</style>
