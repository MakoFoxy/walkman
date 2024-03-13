<template>
  <v-container fluid>
    <v-row>
      <v-col
          cols="1"
          style="text-align: end"
          v-if="isPartner"
      >
        <img src="@/assets/left-arrow.png" width="64" @click="() => SET_USER_OBJECT_BY_INDEX(selectedObjectIndex - 1)">
      </v-col>
      <v-col>
        <table v-if="selectedObject != null">
          <thead>
          <tr>
            <th>Название</th>
            <th>Адрес</th>
            <th>Начало работы</th>
            <th>Окончание работы</th>
            <th>Продолжительность эфира</th>
            <th>Загрузка рекламой</th>
            <th>Загруженность</th>
          </tr>
          </thead>
          <tr>
            <td>
              {{ selectedObject.name }}
            </td>
            <td>
              {{ selectedObject.actualAddress }}
            </td>
            <td>
              {{ selectedObject.beginTime }}
            </td>
            <td>
              {{ selectedObject.endTime }}
            </td>
            <td>
              {{ selectedObject.workTime }}
            </td>
            <td>
              {{ `${selectedObject.uniqueAdvertCount}/${selectedObject.allAdvertCount}/${selectedObject.loading}%` }}
            </td>
            <td>
              <span class="material-icons" :style="`color: ${getOverloadedIconColor(selectedObject)}`">
                stars
              </span>
            </td>
          </tr>
        </table>
      </v-col>
      <v-col
          cols="1"
          style="text-align: start"
          v-if="isPartner"
      >
        <img src="@/assets/right-arrow.png" width="64" @click="() => SET_USER_OBJECT_BY_INDEX(selectedObjectIndex + 1)">
      </v-col>
    </v-row>
  </v-container>
</template>

<script>
import {mapActions, mapGetters} from "vuex";
import {SET_USER_OBJECT_BY_INDEX} from "@/store/actions.type";

export default {
  name: 'ObjectInfoGreenPanel',
  computed: {
    ...mapGetters([
        'currentUser',
        'selectedObjectIndex',
        'isPartner',
    ]),
    selectedObject() {
      return this.currentUser.objects[this.selectedObjectIndex];
    },
  },
  methods: {
    ...mapActions([
      SET_USER_OBJECT_BY_INDEX
    ]),
    getOverloadedIconColor(object) {
      if (object.overloaded) {
        return '#dc1d1d';
      }
      return 'white';
    },
  }
}
</script>

<style scoped>
  table {
    border-collapse:separate;
    border:solid black 1px;
    border-radius:6px;
    -moz-border-radius:6px;
    background-color: #3BC558FF;
    width: 100%;
  }

  td, th {
    border-left:solid black 1px;
    border-top:solid black 1px;
    text-align: center;
  }

  th {
    border-top: none;
    text-align: center;
  }

  td:first-child, th:first-child {
    border-left: none;
  }
</style>
