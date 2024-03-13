<template>
  <div>
    <v-container fluid>
      <v-row no-gutters>
        <v-btn slot="activator" color="primary" class="mb-2" @click="addClient()">Добавить клиента</v-btn>
      </v-row>
      <v-row no-gutters>
        <v-data-table
                fixed-header
                :headers="headers"
                :items="clients" 
                :server-items-length="pagination.totalItems"
                height="70vh"
                style="width:100%;"
                :loading="clientsIsLoading" 
                :no-data-text="'Список пуст'"
                :options.sync="pagination"
                :footer-props="{
                  itemsPerPageOptions: [5, 10, 15, 20, 30, 40, 50],
                  itemsPerPageText: 'Элементов на странице'
                }">
          <template v-slot:item.actions="{ item }">
            <v-tooltip bottom>
              <template v-slot:activator="{ on }">
                <v-icon @click="editItem(item)" v-on="on">create</v-icon>
              </template>
              <span>Редактировать</span>
            </v-tooltip>
          </template>
        </v-data-table>
      </v-row>

    </v-container>
  </div>
</template>

<script>
  import {
    SET_ROWS_PER_PAGE_IN_TABLE
  } from "../store/mutations.type"
  import {
    GET_CLIENTS
  } from '../store/actions.type'

  export default {
    name: 'client-list',
    data: () => ({
      headers: [
        {
          text: 'Название',
          sortable: false,
          value: 'name'
        },
        {
          text: 'Бин',
          sortable: false,
          value: 'bin'
        },
        {
          text: 'ИИК',
          sortable: false,
          value: 'iik'
        },
        {
          text: 'Адрес',
          sortable: false,
          value: 'address'
        },
        {
          text: 'Телефон',
          sortable: false,
          value: 'phone'
        },
        {
          text: 'Действия',
          sortable: false,
          align: 'center',
          value: 'actions'
        }
      ]
    }),
    computed: {
      pagination: {
        get() {
          return {
            rowsPerPage: Number(this.$store.state.system.rowPerPageInTable),
            page: this.$store.state.client.clientPage,
            totalItems: this.$store.state.client.clientsTotalItems
          }
        },
        set(value) {
          this.$store.commit(SET_ROWS_PER_PAGE_IN_TABLE, value.rowsPerPage)
          localStorage.setItem('clientPage', value.page)
          localStorage.setItem('clientRowsPerPage', value.rowsPerPage)
          this.getClient()
        }
      },
      clients: {
        get() {
          return this.$store.state.client.clients
        }
      },
      clientsIsLoading: {
        get(){
          return this.$store.state.client.clientsIsLoading
        }
      }
    },
    methods: {
      editItem(item) {
        this.$router.push(`/edit-client?id=${item.id}`)
      },
      addClient() {
        this.$router.push('/add-client')
      },
      getPage() {
        return localStorage.getItem('clientPage') == null ? this.$store.state.client.clientPage : localStorage.getItem('clientPage')
      },
      getClient() {
        const page = this.getPage()
        const itemsPerPage = this.$store.state.system.rowPerPageInTable
        this.$store.dispatch(GET_CLIENTS, {
          page,
          itemsPerPage
        })
      }
    },
  }
</script>