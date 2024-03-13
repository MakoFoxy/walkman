<template>
  <div>
    <v-container fluid>
      <v-row no-gutters>
        <v-btn slot="activator" color="primary" class="mb-2" @click="addAdmin()">Добавить администратора</v-btn>
      </v-row>
      <v-row no-gutters>
        <v-data-table
                fixed-header
                :headers="headers"
                :items="managers" 
                :server-items-length="pagination.totalItems"
                height="70vh"
                style="width:100%;"
                :loading="managersIsLoading" 
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
    GET_MANAGERS
  } from '../store/actions.type'

  export default {
    name: 'admin-list',
    data: () => ({
      headers: [
        {
          text: 'Фамилия',
          sortable: false,
          value: 'firstName'
        },
        {
          text: 'Имя',
          sortable: false,
          value: 'secondName'
        },
        {
          text: 'Отчество',
          sortable: false,
          value: 'lastName'
        },
        {
          text: 'Номер телефона',
          sortable: false,
          value: 'phoneNumber'
        },
        {
          text: 'Роль',
          sortable: false,
          value: 'role.name'
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
            page: this.$store.state.manager.managerPage,
            totalItems: this.$store.state.manager.managersTotalItems
          }
        },
        set(value) {
          this.$store.commit(SET_ROWS_PER_PAGE_IN_TABLE, value.rowsPerPage)
          localStorage.setItem('managerPage', value.page)
          localStorage.setItem('managerRowsPerPage', value.rowsPerPage)
          this.getManagers()
        }
      },
      managers: {
        get() {
          return this.$store.state.manager.managers
        }
      },
      managersIsLoading: {
        get(){
          return this.$store.state.manager.managersIsLoading
        }
      }
    },
    methods: {
      editItem(item) {
        this.$router.push(`/edit-admin?id=${item.id}`)
      },
      addAdmin() {
        this.$router.push('/add-admin')
      },
      getPage() {
        return localStorage.getItem('adminPage') == null ? this.$store.state.manager.managerPage : localStorage.getItem('adminPage')
      },
      getManagers() {
        const page = this.getPage()
        const itemsPerPage = this.$store.state.system.rowPerPageInTable
        this.$store.dispatch(GET_MANAGERS, {
          page,
          itemsPerPage
        })
      }
    },
  }
</script>