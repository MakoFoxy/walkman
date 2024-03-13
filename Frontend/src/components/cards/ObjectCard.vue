<template>
    <v-container fluid fill-height>
        <v-layout align-center justify-center>
            <v-flex xs12 sm8 md5>
                <v-card class="elevation-12">
                    <v-toolbar dark color="primary">
                        <v-toolbar-title>{{cardType}}</v-toolbar-title>
                        <v-spacer></v-spacer>
                    </v-toolbar>
                    <v-card-text>
                        <v-form>
                            <v-container>
                                <v-layout>
                                    <v-flex xs12 md6>
                                        <v-text-field name="name" label="Название объекта" type="text" v-model="object.name"></v-text-field>
                                    </v-flex>
                                    <v-flex xs12 md6>
                                        <v-text-field name="bin" label="БИН" type="text" v-model="object.bin"></v-text-field>
                                    </v-flex>
                                </v-layout>
                            </v-container>
                            <v-container>
                                <v-layout>
                                    <v-flex xs12 md6>
                                        <v-menu ref="objectBeginTimeMenu" :close-on-content-click="false" v-model="beginTimeMenu"
                                            :nudge-right="40" :return-value.sync="object.beginTime" lazy transition="scale-transition"
                                            offset-y full-width max-width="290px" min-width="290px">
                                            <v-text-field slot="activator" v-model="object.beginTime" label="Начало работы"
                                                prepend-icon="access_time" readonly></v-text-field>
                                            <v-time-picker format="24hr" v-model="object.beginTime" full-width @change="$refs.objectBeginTimeMenu.save(object.beginTime)"></v-time-picker>
                                        </v-menu>
                                    </v-flex>
                                    <v-flex xs12 md6>
                                        <v-menu ref="objectEndTimeMenu" :close-on-content-click="false" v-model="endTimeMenu"
                                            :nudge-right="40" :return-value.sync="object.endTime" lazy transition="scale-transition"
                                            offset-y full-width max-width="290px" min-width="290px">
                                            <v-text-field slot="activator" v-model="object.endTime" label="Окончание работы"
                                                prepend-icon="access_time" readonly></v-text-field>
                                            <v-time-picker format="24hr" v-model="object.endTime" full-width @change="$refs.objectEndTimeMenu.save(object.endTime)"></v-time-picker>
                                        </v-menu>
                                    </v-flex>
                                </v-layout>
                            </v-container>
                            <v-container>
                                <v-layout>
                                    <v-flex xs12 md6>
                                        <v-text-field name="actual-address" label="Фактический адрес" type="text"
                                            v-model="object.actualAddress"></v-text-field>
                                    </v-flex>
                                    <v-flex xs12 md6>
                                        <v-text-field name="legal-address" label="Юридический адрес" type="text"
                                            v-model="object.legalAddress"></v-text-field>
                                    </v-flex>
                                </v-layout>
                            </v-container>
                            <v-container>
                                <v-layout>
                                    <v-flex xs12 md6>
                                        <v-select :items="activityTypes" label="Выберите род деятельности объекта"
                                            v-model="object.activityType" return-object item-value="id" item-text="name"></v-select>
                                    </v-flex>
                                    <v-flex xs12 md6>
                                        <v-select :items="serviceCompanies" label="Выберите обслуживающую компанию"
                                            v-model="object.serviceCompany" return-object item-value="id" item-text="name"></v-select>
                                    </v-flex>
                                </v-layout>
                            </v-container>
                            <v-container>
                                <v-layout>
                                    <v-flex xs12 md6>
                                        <v-text-field name="attendance" label="Посещаемость" type="text" v-model="object.attendance"></v-text-field>
                                    </v-flex>
                                    <v-flex xs12 md6>
                                        <v-text-field name="area" label="Площадь" type="text" v-model="object.area"></v-text-field>
                                    </v-flex>
                                </v-layout>
                            </v-container>
                            <v-container>
                                <v-layout>
                                    <v-flex xs12 md4>
                                        <v-text-field name="renters-count" label="Количество арендаторов" type="text"
                                            v-model="object.rentersCount"></v-text-field>
                                    </v-flex>
                                    <v-flex xs12 md4>
                                        <v-text-field name="silent-percent" label="Процент тишины" type="text" v-model="object.silentPercent"></v-text-field>
                                    </v-flex>
                                    <v-flex xs12 md4>
                                        <v-text-field name="silent-block-interval" label="Интервал тишины в блоках"
                                            type="text" v-model="object.silentBlockInterval"></v-text-field>
                                    </v-flex>
                                </v-layout>
                            </v-container>
                        </v-form>
                    </v-card-text>
                    <v-card-actions>
                        <v-spacer></v-spacer>
                        <upload-btn :color="objectImage == null ? 'red' : 'primary'" title="Загрузить изображение"
                            :fileChangedCallback="fileChanged"></upload-btn>
                        <v-btn color="primary" @click="applyButtonClick">Добавить</v-btn>
                        <v-btn color="primary" @click="$router.push('/home#objects')">Отмена</v-btn>
                    </v-card-actions>
                </v-card>
            </v-flex>
        </v-layout>
    </v-container>
</template>

<script>
    import UploadButton from "vuetify-upload-button";

    export default {
        name: 'add-object',
        components: {
            'upload-btn': UploadButton
        },
        props: {
            cardType: {
                type: String,
                required: true
            },
            object: {
                type: Object,
                required: false
            }
        },
        computed: {

        },
        data: () => ({
            beginTimeMenu: false,
            beginTimeModal: false,
            endTimeMenu: false,
            endTimeModal: false,
            activityTypes: [],
            serviceCompanies: [],
            objectImage: null
        }),
        methods: {
            async addObjectButtonClick() {
                let formData = new FormData()
                formData.append('images', this.objectImage)
                formData.append('addObjectDto', JSON.stringify(this.object))

                const response = await this.axios.post('object', formData, {
                    headers: {
                        'Content-Type': 'multipart/form-data'
                    }
                });

                if (response.status == 200) {
                    this.$router.push('/home#objects')
                }
            },
            fileChanged(file) {
                this.objectImage = file
            }
        },
        async beforeMount() {
            const activityTypePromise = this.axios.get('activity-type');
            const serviceCompanyPromise = this.axios.get('service-company');

            const result = await Promise.all([activityTypePromise, serviceCompanyPromise]);

            result[0].data.forEach(el => {
                this.activityTypes.push(el);
            });

            result[1].data.forEach(el => {
                this.serviceCompanies.push(el);
            });
        }
    }
</script>