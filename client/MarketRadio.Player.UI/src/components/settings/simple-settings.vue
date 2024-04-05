<template>
  <div class="bordered-div">
    <v-container>
      <v-col class="bordered-div" style="padding-right: 20px;">
        <v-row justify="center">
          <h3>Общая громкость</h3>
        </v-row>
        <v-row justify="center">
          <v-slider
            v-model="masterVolume"
            prepend-icon="mdi-volume-high"
            @change="setMasterVolume"
          ></v-slider>
          {{masterVolume}}
        </v-row>
        <v-row justify="center" style="margin-bottom: 10px;">
          <img src="../../images/gear.png" alt="Настройки" width="32" height="32">
          <v-btn style="margin-left: 10px;" @click="openFullSettings">
            Детальные настройки
          </v-btn>
        </v-row>
      </v-col>
      <v-row justify="center">
        <v-btn @click="showAdvertAddDialog = true">
          Добавить рекламу
        </v-btn>
      </v-row>
    </v-container>
    <div class="text-center">
      <v-dialog v-model="showAdvertAddDialog" width="300px">
        <v-card>
          <v-container>
            <v-row justify="center">
              <h3>Зайти в личный кабинет</h3>
            </v-row>
            <v-row justify="center">
              <v-col align="right">
                <v-btn @click="openSite">
                  Да
                </v-btn>
              </v-col>
              <v-col>
                <v-btn @click="showAdvertAddDialog = false">
                  Нет
                </v-btn>
              </v-col>
            </v-row>
          </v-container>
        </v-card>
      </v-dialog>
      <v-dialog v-model="showFullSettingsDialog" fullscreen
                overlay-color="white"
                overlay-opacity="100%"
                transition="dialog-bottom-transition">
        <full-settings :closed="fullSettingsDialogClosed"/>
      </v-dialog>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from 'vue';
import Component from 'vue-class-component';
import FullSettings from '@/components/settings/full-settings.vue';

@Component({
  name: 'simple-settings',
  components: {
    FullSettings,
  },
})
export default class SimpleSettings extends Vue {
  masterVolume = 0

  showAdvertAddDialog = false

  showFullSettingsDialog = false

  async openSite() {
    const url = 'https://localhost/';
    await fetch(`/api/system/open-url?url=${url}`);
    this.showAdvertAddDialog = false;
  }

  async mounted() {
    const response = await fetch('/api/system/master-volume');
    const volume = Number(await response.text());

    if (volume !== this.masterVolume) {
      this.masterVolume = volume;
    }
  }

  setMasterVolume() {
    fetch('/api/system/master-volume', {
      method: 'PUT',
      headers: {
        Accept: 'application/json',
        'Content-Type': 'application/json',
      },
      body: this.masterVolume.toString(),
    });
  }

  openFullSettings() {
    // const routeData = this.$.
    // .resolve({ name: 'settings-page', query: { force: 'true' } });
    // window.open(routeData.href, '_blank');
    // this.$router.push({ name: 'settings-page', query: { force: 'true' } });
    this.showFullSettingsDialog = true;
  }

  fullSettingsDialogClosed() {
    this.showFullSettingsDialog = false;
  }
}
</script>

<style scoped>
.bordered-div {
  padding: 10px;
  border: 1px solid #000;
  border-radius: 15px;
  margin: 10px;
}
</style>
