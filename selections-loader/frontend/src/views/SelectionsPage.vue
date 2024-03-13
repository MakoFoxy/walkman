<template>
  <div>
    <v-container>
      <v-row>
        <v-col>
          <v-container fluid>
            <v-row>
              <v-col class="text-center">
                <h2>Треки</h2>
              </v-col>
            </v-row>
            <v-row>
              <v-col class="text-center">
                <v-tabs center-active optional>
                  <v-tab
                      v-for="genre in reactiveData.genres"
                      :key="genre.id"
                      @click="() => reactiveData.selectedGenre = genre"
                  >
                    {{genre.name}}
                  </v-tab>
                </v-tabs>
              </v-col>
            </v-row>
            <v-row>
              <v-col style="height: 70vh; overflow-y: scroll;">
                <v-container>
                  <v-row v-for="track in tracksInSelectedGenre" :key="track.id">
                    <v-col>
                      <h3>
                        {{ track.name }}
                      </h3>
                    </v-col>
                    <v-col cols="1">
                      <v-btn small @click="() => addTrackToSelection(track)">
                        +
                      </v-btn>
                    </v-col>
                  </v-row>
                </v-container>
              </v-col>
            </v-row>
          </v-container>
        </v-col>
        <v-col cols="1">

        </v-col>
        <v-col>
          <v-container fluid>
            <v-row>
              <v-col class="text-center">
                <h2>Подборки</h2>
              </v-col>
            </v-row>
            <v-row>
              <v-col class="text-center">
                <v-tabs center-active optional>
                  <v-tab
                      v-for="selection in reactiveData.selections"
                      :key="selection.id"
                      @click="() => reactiveData.selectedSelection = selection"
                  >
                    {{selection.name}}
                  </v-tab>
                </v-tabs>
              </v-col>
            </v-row>
            <v-row>
              <v-col style="height: 70vh; overflow-y: scroll;">
                <v-container>
                  <v-row v-for="track in tracksInSelectedSelection" :key="track.id">
                    <v-col>
                      <h3>
                        {{ track.name }}
                      </h3>
                    </v-col>
                    <v-col cols="1">
                      <v-btn small @click="() => removeTrackFromSelection(track)">
                        -
                      </v-btn>
                    </v-col>
                  </v-row>
                </v-container>
              </v-col>
            </v-row>
          </v-container>
          <v-btn
              v-if="reactiveData.selectedSelection != null"
              @click="saveSelection"
          >
            Сохранить
          </v-btn>
        </v-col>
      </v-row>
    </v-container>
  </div>
</template>

<script lang="ts">
import {computed, defineComponent, onBeforeMount, reactive, ref} from '@vue/composition-api';
import Loading from '@/components/Loading.vue';
import {Task} from '@/models/Task';
import {format} from 'date-fns';
import {SimpleModel} from '@/models/SimpleModel';
import {Track} from '@/models/Track';
import {Selection} from '@/models/Selection';
import {TrackInSelection} from '@/models/TrackInSelection';
import {Guid} from '@/helpers/Guid';

export default defineComponent({
  name: 'SelectionsPage',
  components: {Loading},
  setup: () => {
    const tasks = ref(new Array<Task>());
    const reactiveData = reactive({
      genres: new Array<SimpleModel>(),
      tracks: new Array<Track>(),
      selections: new Array<Selection>(),
      selectedGenre: null as SimpleModel | null,
      selectedSelection: null as Selection | null,
    });

    const loadGenres = async () => {
      const genres = await (await fetch('/api/genres')).json();
      reactiveData.genres = genres;
    };

    const loadSelections = async () => {
      const selections = await (await fetch('/api/selections')).json();
      reactiveData.selections = selections;
    };

    const loadTracks = async () => {
      const tracks = await (await fetch('/api/tracks')).json();
      reactiveData.tracks = tracks;
    };

    const tracksInSelectedGenre = computed(() => {
      if (reactiveData.selectedGenre == null || reactiveData.tracks.length === 0) {
        return [];
      }

      return reactiveData.tracks.filter((t) => t.genres.some((g) => g.id == reactiveData.selectedGenre?.id));
    });

    const tracksInSelectedSelection = computed(() => {
      if (reactiveData.selectedSelection == null || reactiveData.tracks.length === 0) {
        return [];
      }

      return reactiveData.selectedSelection!.tracks;
    });

    onBeforeMount(async () => {
      await loadGenres();
      await loadTracks();
      await loadSelections();

      const response = await fetch('/api/tasks');
      const json = await response.json() as [];

      const taskArray = new Array<Task>();

      json.forEach((j: any) => {
        taskArray.push(new Task({
          name: j.name,
          taskType: j.taskType,
          id: j.id,
          priority: j.priority,
          createDate: new Date(j.createDate),
          finishDate: j.finishDate == null ? null : new Date(j.finishDate),
          isFinished: j.isFinished,
          taskObjectId: j.taskObjectId,
        }));
      });

      tasks.value = taskArray;
    });

    const formatLogical = (logical: boolean) => {
      return logical ? 'Да' : 'Нет';
    };

    const formatDate = (date: Date | null) => {
      if (date == null) {
        return '-';
      }

      return format(date, 'HH:mm dd.MM.yyyy');
    };

    const removeTrackFromSelection = (track: Track) => {
      reactiveData.selectedSelection!.tracks = reactiveData.selectedSelection!.tracks.filter((t) => t.id != track.id);
    };

    const addTrackToSelection = (track: Track) => {
      reactiveData.selectedSelection!.tracks.push(new TrackInSelection({
        id: track.id,
        name: track.name,
      }));
    };

    const saveSelection = async () => {
      const response = await fetch('/api/selections', {
        method: reactiveData.selectedSelection!.id === Guid.empty ? 'POST' : 'PUT',
        body: JSON.stringify(reactiveData.selectedSelection),
        headers: {
          Accept: 'application/json',
          'Content-Type': 'application/json',
        },
      });

      if (response.ok) {
        await loadSelections();
      }
    };

    return {
      tasks,
      reactiveData,
      tracksInSelectedGenre,
      tracksInSelectedSelection,
      formatLogical,
      formatDate,
      removeTrackFromSelection,
      addTrackToSelection,
      saveSelection,
    };
  },
});
</script>

<style lang="stylus" scoped>

</style>
