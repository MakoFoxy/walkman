<template>
  <v-container fluid>
    <v-row>
      <v-col cols="1"></v-col>
      <v-col>
        <v-row v-for="obj in objects" v-bind:key="obj.id" align="center" justify="center" style="cursor: pointer;" @click="() => objectSelected(obj)">
          <div style="width: 100%" class="bordered-div">
            <h4 class="text-center">
              {{obj.name}}
            </h4>
          </div>
        </v-row>
      </v-col>
      <v-col cols="1"></v-col>
    </v-row>
  </v-container>
</template>

<script lang="ts">
import Vue from 'vue';
import Component from 'vue-class-component';
import { getModule } from 'vuex-module-decorators';
import axios from 'axios';
import SimpleModel from '@/shared/models/SimpleModel';
import ObjectModule from '@/store/modules/object-module/object-module';
import PlaylistModule from '@/store/modules/playlist-module/playlist-module';
import Playlist from '@/shared/models/PlaylistModel';

    @Component({
      name: 'object-selector',
    })
export default class ObjectSelector extends Vue {
        objects: SimpleModel[] = []

        objectModule = getModule(ObjectModule, this.$store);

        playlistModule = getModule(PlaylistModule, this.$store);

        async mounted() {
          const response = await axios.get('/object/all');
          this.objects = response.data;
        }

        async objectSelected(selectedObject: SimpleModel) {
          const response = await axios.post(`/object/${selectedObject.id}`);
          const object = response.data;

          await this.objectModule.changeObject(object);
          await this.playlistModule.changePlaylist(new Playlist());
          this.$router.push({ name: 'main' });
        }
}
</script>

<style scoped>
.bordered-div {
  padding: 10px;
  border: 1px solid #000;
  border-radius: 15px;
  margin: 2px;
}
</style>
