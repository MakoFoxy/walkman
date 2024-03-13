<template>
    <div>
        <v-container fluid fill-height>
            <v-layout align-center justify-center>
                <v-flex xs12 sm12 md12>
                    <v-card class="elevation-12">
                        <v-toolbar dark color="primary">
                            <v-toolbar-title>Добавление рекламы</v-toolbar-title>
                            <v-spacer></v-spacer>
                        </v-toolbar>
                        <v-card-text>
                            <v-form v-model="valid" lazy-validation ref="form">
                                <v-layout row>
                                    <v-flex xs6 sm6 md6>
                                        <v-text-field :rules="requiredRule" name="name" label="Название рекламы"
                                            type="text" v-model.trim="advert.name"></v-text-field>
                                    </v-flex>
                                    <v-flex xs6 sm6 md6>
                                        <v-file-input :placeholder="advertFile == null ? 'Файл не загружен' : 'Выбран файл'"
                                            accept="audio/*" @change="fileChanged" :showSize="true">
                                        </v-file-input>
                                    </v-flex>
                                </v-layout>
                                <v-layout row justify-space-between>
                                    <v-flex xs6 sm6 md6>
                                        <v-menu ref="advertDateBeginMenu" :close-on-content-click="false"
                                            v-model="dateBeginMenu" :nudge-right="40"
                                            :return-value.sync="advert.dateBegin" transition="scale-transition" offset-y
                                            max-width="290px" min-width="290px">
                                            <template v-slot:activator="{ on }">
                                                <v-text-field :value="computedDateBeginFormatted" clearable
                                                    label="Дата начала" prepend-icon="event" readonly v-on="on">
                                                </v-text-field>
                                            </template>
                                            <v-date-picker v-model="advert.dateBegin" no-title scrollable>
                                                <v-spacer></v-spacer>
                                                <v-btn text color="primary" @click="dateBeginMenu = false">Закрыть
                                                </v-btn>
                                                <v-btn text color="primary"
                                                    @click="$refs.advertDateBeginMenu.save(advert.dateBegin)">OK</v-btn>
                                            </v-date-picker>
                                        </v-menu>
                                    </v-flex>
                                    <v-flex xs6 sm6 md6>

                                        <v-menu ref="advertDateEndMenu" :close-on-content-click="false"
                                            v-model="dateEndMenu" :nudge-right="40" :return-value.sync="advert.dateEnd"
                                            transition="scale-transition" offset-y max-width="290px" min-width="290px">
                                            <template v-slot:activator="{ on }">
                                                <v-text-field :value="computedDateEndFormatted" clearable
                                                    label="Дата окончания" prepend-icon="event" readonly v-on="on">
                                                </v-text-field>
                                            </template>
                                            <v-date-picker v-model="advert.dateEnd" no-title scrollable>
                                                <v-spacer></v-spacer>
                                                <v-btn text color="primary" @click="dateEndMenu = false">Закрыть</v-btn>
                                                <v-btn text color="primary"
                                                    @click="$refs.advertDateEndMenu.save(advert.dateEnd)">OK</v-btn>
                                            </v-date-picker>
                                        </v-menu>
                                    </v-flex>
                                </v-layout>

                                <v-layout row justify-space-between>
                                    <v-flex xs6 sm6 md6>
                                        <v-select :items="[5, 10, 15, 20]" label="Количество выходов"
                                            v-model="advert.repeatCount" :rules="requiredRule" required></v-select>
                                    </v-flex>
                                    <v-flex v-show="advertTypeSelectable" xs6 sm6 md6>
                                        <v-select :items="advertTypes" label="Выберите тип рекламы"
                                            v-model="advert.advertType" return-object item-value="id" item-text="name"
                                            :rules="requiredRule" required></v-select>
                                    </v-flex>

                                    <v-flex xs6 sm6 md6>
                                        <v-select :items="clients" label="Выберите клиента" v-model="advert.client"
                                            return-object item-value="id" item-text="name" :rules="requiredRule">
                                        </v-select>
                                    </v-flex>
                                    <v-flex v-show="packageTypeSelectable" xs6 sm6 md6>
                                        <v-select :items="packageTypes" label="Тип пакета"
                                            v-model="advert.packageTypeObject" return-object item-value="id"
                                            item-text="name" :rules="requiredRule" required></v-select>
                                    </v-flex>
                                </v-layout>

                                <v-layout row justify-space-between wrap
                                    v-show="advert.packageTypeObject == null || advert.packageTypeObject.id == 'Regular'">
                                    <v-flex xs12 sm12 md12>
                                        <v-text-field label="Фильтр объектов" prepend-icon="search" v-debounce="findObject">
                                        </v-text-field>
                                    </v-flex>
                                    <v-flex v-for="(gObj, i) in groupedObjects"
                                        v-bind:key="groupedObjects[i].map(g => g.id).toString()"
                                        :class="groupedObjects[i].map(g => g.id).toString()">
                                        <v-checkbox v-for="obj in gObj"
                                            v-show="obj.isVisible"
                                            v-bind:key="obj.id"
                                            :label="obj.name"
                                            :value="obj.id"
                                            @change="() => objectChanged(obj.id)">
                                        </v-checkbox>
                                    </v-flex>
                                </v-layout>
                            </v-form>
                        </v-card-text>
                        <v-card-actions>
                            <v-spacer></v-spacer>
                            <v-btn color="primary" @click="addAdvertButtonClick" :disabled="loadingInProgress">Добавить
                            </v-btn>
                            <v-btn color="primary" @click="cancelButtonClick">Отмена</v-btn>
                        </v-card-actions>
                    </v-card>
                </v-flex>
            </v-layout>
        </v-container>

        <div class="text-xs-center">
            <v-dialog v-model="loadingInProgress" hide-overlay persistent width="300">
                <v-card color="primary" dark>
                    <v-card-text>
                        Идет загрузка, пожалуйста ожидайте
                        <v-progress-linear indeterminate color="white" class="mb-0"></v-progress-linear>
                    </v-card-text>
                </v-card>
            </v-dialog>
        </div>

        <div class="text-xs-center">
          <v-dialog v-model="showAlert" hide-overlay persistent width="50vw">
            <v-card color="warning" dark>
              <v-card-title>
                <v-container fluid>
                  <v-row>
                    <v-col class="text-center" cols="12">
                      <h3>
                        Внимание!
                      </h3>
                      <span>
                        Реклама будет добавлена на объекты не в полном объеме!
                      </span>
                    </v-col>
                  </v-row>
                </v-container>
              </v-card-title>
              <v-card-text>
                <v-container fluid>
                  <v-row
                      class="text-center"
                      style="font-size: 1rem; color: white;"
                  >
                    <v-col cols="3">
                      <span>
                       Дата выхода
                      </span>
                    </v-col>
                    <v-col cols="4">
                      <span>
                       Объект
                      </span>
                    </v-col>
                    <v-col>
                      <span>
                       Количество рекламы невошедшей в эфир
                      </span>
                    </v-col>
                  </v-row>
                  <v-row
                      class="text-center"
                      style="font-size: 1rem; color: white;"
                      v-for="data in alertData" :key="data.date.toString() + data.object.id"
                  >
                    <v-col cols="3">
                      <span>
                       {{ new Date(data.date).toLocaleDateString() }}
                      </span>
                    </v-col>
                    <v-col cols="4">
                      <span>
                       {{ data.object.name }}
                      </span>
                    </v-col>
                    <v-col>
                      <span>
                       {{ data.overflow }} из {{ advert.repeatCount }}
                      </span>
                    </v-col>
                  </v-row>
                </v-container>
              </v-card-text>
              <v-card-actions>
                <v-container>
                  <v-row>
                    <v-spacer></v-spacer>
                    <v-col class="text-end">
                      <v-btn
                          color="primary"
                          @click="addAdvert"
                      >
                        Загрузить
                      </v-btn>
                    </v-col>
                    <v-col>
                      <v-btn
                          light
                          @click="showAlert = false"
                      >
                        Отменить
                      </v-btn>
                    </v-col>
                    <v-spacer></v-spacer>
                  </v-row>
                </v-container>
              </v-card-actions>
            </v-card>
          </v-dialog>
        </div>
    </div>
</template>

<script>
    import moment from 'moment';
    import groupBy from 'lodash/groupBy';
    import {PartnerAccessToObject} from "@/helpers/permissions";
    import userHomePage from "@/helpers/UserHomePage";

    export default {
        name: 'add-advert',
        data: () => ({
            valid: true,
            requiredRule: [
                v => !!v || 'Поле обязательно для заполнения'
            ],
            alertData: [],
            loadingInProgress: false,
            showAlert: false,
            dateBeginMenu: false,
            dateBeginModal: false,
            dateEndMenu: false,
            dateEndModal: false,
            advert: {
                name: '',
                dateBegin: '',
                dateEnd: '',
                advertType: null,
                objects: [],
                repeatCount: null,
                client: null,
                packageType: null,
                packageTypeObject: null,
            },
            selectedTrackLength: 0,
            advertTypes: [],
            advertFile: undefined,
            objects: [],
            clients: [],
            packageTypes: [{
                id: 'None',
                name: 'Не выбрано'
            }, {
                id: 'Regular',
                name: 'Обычный'
            }, {
                id: 'Cheap',
                name: 'Дешевый'
            }],
            packageTypeSelectable: true,
            advertTypeSelectable: true,
        }),
        methods: {
            findObject(text) {
                if (text === ''){
                    this.objects.forEach(o => o.isVisible = true);
                    return;
                }
                const wrongObjects = this.objects.filter(o => o.name.toLowerCase().indexOf(text.toLowerCase()) === -1);
                wrongObjects.forEach(o => o.isVisible = false);
                this.objects.filter(o => !wrongObjects.includes(o)).forEach(o => o.isVisible = true);
            },
            objectChanged(objectId) {
                const obj = this.objects.find(o => o.id === objectId)
                const exists = this.advert.objects.includes(obj)

                if (!exists) {
                    this.advert.objects.push(obj)
                } else {
                    this.advert.objects = this.advert.objects.filter(o => o !== obj)
                }
            },
            cancelButtonClick(){
              this.$router.push(`/${userHomePage.defaultPage}#adverts`);
            },
            async addAdvertButtonClick() {
                if (!this.$refs.form.validate()) {
                    return
                }

                this.loadingInProgress = true
                const possibilityResponse = await this.axios.get('adverts/check-possibility', {
                  params: {
                    advertLength: this.selectedTrackLength,
                    repeatCount: this.advert.repeatCount,
                    dateBegin: this.advert.dateBegin,
                    dateEnd: this.advert.dateEnd,
                    objects: this.advert.objects.map((o) => o.id),
                  }
                })

                if (possibilityResponse.data.problems.length > 0) {
                  this.loadingInProgress = false
                  this.alertData = possibilityResponse.data.problems
                  this.showAlert = true
                } else {
                  await this.addAdvert()
                }
            },
            async addAdvert() {
              if (!this.$refs.form.validate()) {
                return
              }

              this.advert.packageType = this.advert.packageTypeObject.id

              if (this.advert.packageTypeObject.id === 'Cheap') {
                this.advert.objects = []
              }

              let formData = new FormData()
              formData.append('advertFile', this.advertFile)
              formData.append('advert', JSON.stringify(this.advert))
              this.loadingInProgress = true

              try {
                await this.axios.post('adverts', formData, {
                  headers: {
                    'Content-Type': 'multipart/form-data'
                  }
                })
              } catch (err) {
                this.loadingInProgress = false
                return
              }

              this.$router.push(`/${userHomePage.defaultPage}#adverts`)
            },
            fileChanged(file) {
                this.advertFile = file

                if (file == null) {
                  this.selectedTrackLength = 0
                  return
                }

                const audio = new Audio();
                audio.src = URL.createObjectURL(file);
                audio.onloadedmetadata = () => {
                  this.selectedTrackLength = audio.duration
                }
                audio.load()
            },
        },
        async beforeMount() {
            this.advert.packageTypeObject = this.packageTypes[1];

            this.axios.get('advert-types').then((response) => {
                this.advertTypes = response.data;
                if (this.$store.state.user.user.permissions.includes(PartnerAccessToObject)){
                  this.advert.advertType = this.advertTypes.find(at => at.code === 'Own');
                  this.advertTypeSelectable = false;
                  this.packageTypeSelectable = false;
                }
            });
            this.axios.get('object/all')
            .then((response) => response.data.map(c => {
                return {...c, isVisible: true };
            }))
            .then((objects) => {
                this.objects = objects;
            });
            this.axios.get('organizations?page=1&itemsPerPage=10000&isDictionary=true').then((response) => {
                this.clients = response.data.result;
            });
        },
        computed: {
            computedDateBeginFormatted() {
                return this.advert.dateBegin ? moment(this.advert.dateBegin).format('DD.MM.YYYY') : ''
            },
            computedDateEndFormatted() {
                return this.advert.dateEnd ? moment(this.advert.dateEnd).format('DD.MM.YYYY') : ''
            },
            dateBeginRules() {
                const rules = []

                const rule = v => {
                    if (v === '') {
                        return 'Дата начала обязательна'
                    }

                    if (moment(v) < moment()) {
                        return `Дата начала должна быть больше чем ${moment().format('DD.MM.YYYY').toString()}`
                    }

                    if (moment(this.advert.dateBegin) >= moment(this.advert.dateEnd)) {
                        return 'Дата начала должна быть меньше даты окончания'
                    }

                    return true
                }
                rules.push(rule)

                return rules
            },
            dateEndRules() {
                const rules = []

                const rule = v => {
                    if (v === '') {
                        return 'Дата окончания обязательна'
                    }

                    if (moment(this.advert.dateBegin) >= moment(this.advert.dateEnd)) {
                        return 'Дата окончания должна быть больше даты начала'
                    }

                    return true
                }
                rules.push(rule)

                return rules
            },
            objectRequiredRule() {
                const rules = []

                const rule = v => {
                    if (!!v && (this.advert.packageTypeObject == null && this.advert.packageTypeObject.id ===
                            'Regular')) {
                        return 'Поле обязательно для заполнения'
                    }

                    return true
                }
                rules.push(rule)

                return rules

            },
            groupedObjects() {
                return groupBy(this.objects, 'priority');
            }
        }
    }
</script>
