<template>
  <div>
    <v-container>
      <v-row>
        <v-col>
          <v-btn
              @click="dialogIsOpen = true"
          >
            Добавить жанр
          </v-btn>
        </v-col>
      </v-row>
      <v-row>
        <v-col>
          <v-simple-table dense style="height: 80vh; overflow-y: auto">
            <template v-slot:default>
              <thead>
              <tr>
                <th class="text-left">Название</th>
              </tr>
              </thead>
              <tbody>
              <tr v-for="genre in reactiveData.genres" :key="genre.id">
                <td class="text-left">{{ genre.name }}</td>
              </tr>
              </tbody>
            </template>
          </v-simple-table>
        </v-col>
      </v-row>
    </v-container>

    <div class="text-center">
      <v-dialog
          width="80vw"
          :scrollable="false"
          v-model="dialogIsOpen"
          hide-overlay
          persistent
      >
        <v-card>
          <v-card-title class="headline grey lighten-2">
            Добавление жанра
          </v-card-title>

          <v-card-text>
            <v-container>
              <v-row>
                <v-col>
                  <v-form>
                    <v-text-field
                        v-model="reactiveData.genre.name"
                    ></v-text-field>
                  </v-form>
                </v-col>
              </v-row>
            </v-container>
          </v-card-text>

          <v-divider></v-divider>

          <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn
                color="primary"
                text
                @click="addGenre"
            >
              Добавить
            </v-btn>
            <v-spacer></v-spacer>
            <v-btn
                color="primary"
                text
                @click="dialogIsOpen = false"
            >
              Отменить
            </v-btn>
            <v-spacer></v-spacer>
          </v-card-actions>
        </v-card>
      </v-dialog>
    </div>
  </div>
</template>

<script lang="ts">
import {defineComponent, onBeforeMount, reactive, ref} from '@vue/composition-api';
import {SimpleModel} from '@/models/SimpleModel';

export default defineComponent({
  name: 'GenresPage',
  setup: () => {
    const dialogIsOpen = ref(false);
    const reactiveData = reactive({
      genre: {
        name: '',
      },
      genres: new Array<SimpleModel>(),
    });

    const loadGenres = async () => {
      const genres = await (await fetch('/api/genres')).json();
      reactiveData.genres = genres;
    };

    onBeforeMount(async () => {
      await loadGenres();
    });

    const addGenre = async () => {
      const response = await fetch('/api/genres', {
        method: 'POST',
        headers: {
          Accept: 'application/json',
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          ...reactiveData.genre,
        }),
      });

      if (response.ok) {
        dialogIsOpen.value = false;
        await loadGenres();
      }
    };

    return {
      reactiveData,
      dialogIsOpen,
      addGenre,
    };
  },
});
</script>

<style lang="stylus" scoped>

</style>
