<template>
  <div class="parent">
    <div class="child">
      <h1>Загрузка...</h1>
    </div>
  </div>
</template>

<script lang="ts">
import {defineComponent} from '@vue/composition-api';

export default defineComponent({
  name: 'Loading',
});
</script>

<style scoped>
.parent {
  height: 100vh;
  align-items: center;
  display: flex;
  justify-content: center;
  flex-direction: column;
}

.child {
  display: flex;
  align-items: center;
}
</style>
