<template>
    <object-list/>
</template>

<script>
import ObjectList from '@/components/ObjectList.vue'

export default {
  name: 'object-list-page',
  data: () => ({
    objects: []
  }),
  components: {
    ObjectList
  }
}
</script>
