<template>
  <div>
    <v-container>
      <v-row>
        <v-col>
          <v-btn
              @click="openFolder"
          >
            Выбрать папку подборку
          </v-btn>
        </v-col>
        <v-col>
          <v-menu
              ref="menuBegin"
              v-model="menuBeginIsOpen"
              :close-on-content-click="false"
              :return-value.sync="reactiveData.dateBegin"
              transition="scale-transition"
              offset-y
              min-width="auto"
          >
            <template v-slot:activator="{ on, attrs }">
              <v-text-field
                  :value="dateBeginFormat"
                  label="Выберите дату начала"
                  prepend-icon="mdi-calendar"
                  readonly
                  v-bind="attrs"
                  v-on="on"
              ></v-text-field>
            </template>
            <v-date-picker
                v-model="reactiveData.dateBegin"
                no-title
                scrollable
                locale="ru-ru"
                :first-day-of-week="1"
            >
              <v-spacer></v-spacer>
              <v-btn
                  text
                  color="primary"
                  @click="menuBeginIsOpen = false"
              >
                Отмена
              </v-btn>
              <v-btn
                  text
                  color="primary"
                  @click="$refs.menuBegin.save(reactiveData.dateBegin)"
              >
                OK
              </v-btn>
            </v-date-picker>
          </v-menu>
        </v-col>
        <v-col>
          <v-menu
              ref="menuEnd"
              v-model="menuEndIsOpen"
              :close-on-content-click="false"
              :return-value.sync="reactiveData.dateEnd"
              transition="scale-transition"
              offset-y
              min-width="auto"
          >
            <template v-slot:activator="{ on, attrs }">
              <v-text-field
                  :value="dateEndFormat"
                  label="Выберите дату окончания"
                  prepend-icon="mdi-calendar"
                  readonly
                  v-bind="attrs"
                  v-on="on"
              ></v-text-field>
            </template>
            <v-date-picker
                v-model="reactiveData.dateEnd"
                no-title
                scrollable
                locale="ru-ru"
                :first-day-of-week="1"
            >
              <v-spacer></v-spacer>
              <v-btn
                  text
                  color="primary"
                  @click="menuEndIsOpen = false"
              >
                Отмена
              </v-btn>
              <v-btn
                  text
                  color="primary"
                  @click="$refs.menuEnd.save(reactiveData.dateEnd)"
              >
                OK
              </v-btn>
            </v-date-picker>
          </v-menu>
        </v-col>
        <v-col>
          <v-combobox
              return-object
              :items="reactiveData.genres"
              item-text="name"
              item-value="id"
              v-model="reactiveData.tracksGenre"
          ></v-combobox>
        </v-col>
        <v-col
            class="text-end"
        >
          <v-btn
          :disabled="reactiveData.data == null || reactiveData.tracksGenre == null"
          @click="createSelection"
          >
            Сохранить подборку
          </v-btn>
        </v-col>
      </v-row>
      <v-row
          v-if="reactiveData.data != null"
          class="text-center"
      >
        <v-col>
          <h2>
            Будет создана подборка с названием "{{ reactiveData.data.selectionName }}"
          </h2>
        </v-col>
      </v-row>
      <v-row>
        <v-col
            class="text-center"
        >
          <h4>
            Данные треки будут загружены на сервер
          </h4>
        </v-col>
      </v-row>
      <v-row
          v-if="reactiveData.data != null"
      >
        <v-col>
          <v-simple-table dense style="height: 75vh; overflow-y: auto">
            <template v-slot:default>
              <thead>
              <tr>
                <th
                    class="text-center"
                >
                  Название
                </th>
              </tr>
              </thead>
              <tbody>
              <tr
                  v-for="track in reactiveData.data.tracks"
                  :key="track"
              >
                <td
                    class="text-center"
                >
                  {{ track }}
                </td>
              </tr>
              </tbody>
            </template>
          </v-simple-table>
        </v-col>
      </v-row>
    </v-container>
  </div>
</template>

<script lang="ts">
import {computed, defineComponent, onBeforeMount, reactive, ref} from '@vue/composition-api';
import {SelectionFolderOpenResult} from '@/models/SelectionFolderOpenResult';
import {format, lastDayOfYear, parseISO, startOfYear} from 'date-fns';
import {Container} from '@/Container';
import {Store} from '@/store';

export default defineComponent({
  name: 'CreateSelectionsFromFolderPage',
  setup: () => {
    const menuBeginIsOpen = ref(false);
    const menuEndIsOpen = ref(false);

    const reactiveData = reactive({
      data: null as SelectionFolderOpenResult | null,
      dateBegin: format(startOfYear(Date.now()), 'yyyy-MM-dd'),
      dateEnd: format(lastDayOfYear(Date.now()), 'yyyy-MM-dd'),
      genres: [],
      tracksGenre: null,
    });

    const dateBeginFormat = computed(() => format(parseISO(reactiveData.dateBegin), 'dd.MM.yyyy'));
    const dateEndFormat = computed(() => format(parseISO(reactiveData.dateEnd), 'dd.MM.yyyy'));

    onBeforeMount(async () => {
      const genres = await (await fetch('/api/genres')).json();
      reactiveData.genres = genres;
    });

    const openFolder = async () => {
      const response = await fetch('/api/system/open-folder', {
        method: 'POST'
      });

      if (response.status === 204) {
        reactiveData.data = null;
        return;
      }

      reactiveData.data = await response.json();
    };

    const createSelection = async () => {
      const data = JSON.stringify({
        fullPath: reactiveData.data!.fullPath,
        dateBegin: reactiveData.dateBegin,
        dateEnd: reactiveData.dateEnd,
        genre: reactiveData.tracksGenre,
      });
      const response = await fetch('/api/selections/from-folder', {
        method: 'POST',
        body: data,
        headers: {
          Accept: 'application/json',
          'Content-Type': 'application/json',
        },
      });

      if (response.ok) {
        alert('Успешно загружено');
        reactiveData.tracksGenre = null;
        reactiveData.data = null;
      }
    };

    return {
      menuBeginIsOpen,
      menuEndIsOpen,
      reactiveData,
      dateBeginFormat,
      dateEndFormat,
      openFolder,
      createSelection,
    };
  }
});
</script>

<style lang="stylus" scoped>

</style>
