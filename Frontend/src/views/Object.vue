<template>
    <Object />
</template>

<script>
import Object from '@/components/Object.vue'

export default {
  name: 'object',
  data: () => ({
    object: {}
  }),
  components: {
    Object
  },
  beforeMount : async function(){
  }
}
</script>
