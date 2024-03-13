<template>
  <div>
    <v-container>
      <v-row>
        <v-col>
          <v-btn
              @click="trackDialogIsOpen = true"
          >
            Добавить трек
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
                  <th class="text-center">Продолжительность</th>
                  <th class="text-center">Загружено на сервер?</th>
                  <th class="text-center">Загрузка на сервер в процессе?</th>
                  <th class="text-center">Жанры</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="track in tracks" :key="track.id">
                  <td class="text-left">{{ track.name }}</td>
                  <td class="text-center">{{ formatLength(track) }}</td>
                  <td class="text-center">{{ formatLogical(track.uploaded) }}</td>
                  <td class="text-center">{{ formatLogical(track.uploadInProgress) }}</td>
                  <td class="text-center">{{ genreNames(track) }}</td>
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
          v-model="trackDialogIsOpen"
          hide-overlay
          persistent
      >
        <v-card>
          <v-card-title class="headline grey lighten-2">
            Добавление треков
          </v-card-title>

          <v-card-text>
            <v-container>
              <v-row>
                <v-col>
                  <v-form>
                    <v-file-input
                        multiple
                        counter
                        show-size
                        accept=".mp3"
                        v-model="reactiveData.files"
                    ></v-file-input>
                    <v-combobox
                        return-object
                        :items="reactiveData.genres"
                        item-text="name"
                        item-value="id"
                        v-model="reactiveData.trackGenres"
                        multiple
                    ></v-combobox>
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
                @click="addTracks"
            >
              Добавить
            </v-btn>
            <v-spacer></v-spacer>
            <v-btn
                color="primary"
                text
                @click="trackDialogIsOpen = false"
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
import { addSeconds, format } from 'date-fns';
import {Track} from '@/models/Track';
import {SimpleModel} from '@/models/SimpleModel';

export default defineComponent({
  name: 'TracksPage',
  setup: () => {
    const loading = ref(true);
    const trackDialogIsOpen = ref(false);
    const tracks = ref(new Array<Track>());
    const reactiveData = reactive({
      files: [] as File[],
      trackGenres: [],
      genres: new Array<SimpleModel>(),
    });

    const loadTracks = async () => {
      const response = await fetch('/api/tracks');
      const jsonObject = await response.json();

      tracks.value = jsonObject;
    };

    onBeforeMount(async () => {
      const genres = await (await fetch('/api/genres')).json();
      reactiveData.genres = genres;
      await loadTracks();
    });

    const genreNames = (track: Track) => {
      return track.genres.map((g) => g.name).join(' ');
    };

    const formatLength = (track: Track) => {
      return track.length;
    };

    const formatLogical = (logical: boolean) => {
      return logical ? 'Да' : 'Нет';
    };

    const addTracks = async () => {
      const files = reactiveData.files.map((f) => (f as any).path);

      const response = await fetch('/api/tracks', {
        method: 'POST',
        body: JSON.stringify({
          paths: files,
          genres: reactiveData.trackGenres,
        }),
        headers: {
          Accept: 'application/json',
          'Content-Type': 'application/json',
        },
      });

      if (response.ok) {
        trackDialogIsOpen.value = false;
        reactiveData.trackGenres = [];
        reactiveData.files = [];
        await loadTracks();
      }
    };

    return {
      reactiveData,
      trackDialogIsOpen,
      loading,
      tracks,
      genreNames,
      formatLength,
      formatLogical,
      addTracks,
    };
  },
});
</script>

<style lang="stylus" scoped>

</style>
