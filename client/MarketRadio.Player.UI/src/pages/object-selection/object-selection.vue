<template>
  <v-container>
    <v-row align="center" justify="center">
      <v-col cols="1"></v-col>
      <v-col>
        <h2 class="text-center">Выберите объект</h2>
      </v-col>
      <v-col cols="1"></v-col>
    </v-row>
    <v-row>
      <object-selector />
    </v-row>
  </v-container>
</template>

<script lang="ts">
import Vue from 'vue';
import Component from 'vue-class-component';
import ObjectSelector from '@/components/object-selector/object-selector.vue';

@Component({
  name: 'object-selection',
  components: {
    ObjectSelector,
  },
})
export default class ObjectSelection extends Vue {
}
</script>

<style scoped>

</style>
