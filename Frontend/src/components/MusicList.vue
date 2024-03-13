<template>
  <div>
    <v-btn slot="activator" color="primary" class="mb-2" @click="addMusic()">Добавить музыкальный трек</v-btn>
    <v-data-table 
        :headers="headers" 
        :items="music" 
        :total-items="pagination.totalItems" 
        class="elevation-1 walkman-table" 
        :loading="musicIsLoading"
        :no-data-text="'Список пуст'"
        :rows-per-page-items="[5, 10, 15, 20, 30, 40, 50]" 
        :pagination.sync="pagination"
        rows-per-page-text="Элементов на странице">

      <v-progress-linear slot="progress" color="blue" indeterminate></v-progress-linear>

      <template slot="items" slot-scope="props">
        <tr @click="editItem(props.item)">
          <td>{{ props.item.name }}</td>
          <td>{{ props.item.length }}</td>
        </tr>
      </template>
    </v-data-table>
  </div>
</template>

<script>
  import {
    SET_ROWS_PER_PAGE_IN_TABLE
  } from "../store/mutations.type"
  import {
    GET_MUSIC
  } from '../store/actions.type'

  export default {
    name: 'music-list',
    data: () => ({
      headers: [
        {
          text: 'Название',
          value: 'name'
        },
        {
          text: 'Продолжительность',
          value: 'length'
        }
      ],
      loadingInProgress: false
    }),
    computed: {
      pagination: {
        get() {
          return {
            rowsPerPage: Number(this.$store.state.system.rowPerPageInTable),
            page: this.$store.state.music.musicPage,
            totalItems: this.$store.state.music.musicTotalItems
          }
        },
        set(value) {
          this.$store.commit(SET_ROWS_PER_PAGE_IN_TABLE, value.rowsPerPage)
          localStorage.setItem('musicPage', value.page)
          localStorage.setItem('musicRowsPerPage', value.rowsPerPage)
          this.getMusic()
        }
      },
      music: {
        get() {
          return this.$store.state.music.music
        }
      },
      musicIsLoading: {
        get(){
          return this.$store.state.music.musicIsLoading
        }
      }
    },
    methods: {
      editItem() {},
      addMusic() {
        this.$router.push('/add-music')
      },
      getPage() {
        return localStorage.getItem('musicPage') == null ? this.$store.state.music.musicPage : localStorage.getItem('musicPage')
      },
      getMusic() {
        const page = this.getPage()
        const itemsPerPage = this.$store.state.system.rowPerPageInTable
        this.$store.dispatch(GET_MUSIC, {
          page,
          itemsPerPage
        })
      }
    },
  }
</script>