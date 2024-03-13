<template>
    <v-container fluid fill-height>
        <v-layout align-center justify-center>
            <v-flex xs12 sm12 md12>
                <v-card>
                    <v-toolbar dark color="primary">
                        <v-toolbar-title>Добавление объекта</v-toolbar-title>
                        <v-spacer></v-spacer>
                    </v-toolbar>
                    <v-card-text>
                        <v-form ref="form">
                            <v-container>
                                <v-layout>
                                    <v-flex>
                                        <v-combobox :items="cities" v-model="object.city" return-object item-value="id" item-text="name" label="Выберите город" autocomplete="disabled"></v-combobox>
                                    </v-flex>
                                    <v-flex>
                                        <v-text-field name="name" label="Название объекта" type="text" v-model.trim="object.name" :rules="requiredRule" autocomplete="disabled"></v-text-field>
                                    </v-flex>
                                    <v-flex>
                                        <v-text-field name="priority" label="Колонка при добавлении рекламы, например: 2" type="number" v-model="object.priority" :rules="priorityRule" autocomplete="disabled"></v-text-field>
                                    </v-flex>
                                </v-layout>
                            </v-container>
                            <v-container>
                                <v-layout>
                                    <v-flex xs12 md6>
                                        <v-text-field name="actual-address" label="Фактический адрес" type="text"
                                            v-model.trim="object.actualAddress" :rules="requiredRule" autocomplete="disabled"></v-text-field>
                                    </v-flex>
                                    <v-flex xs12 md6>
                                        <v-text-field name="bin" label="БИН, например: 130740015900" type="text" v-model.trim="object.bin" :rules="requiredRule" autocomplete="disabled"></v-text-field>
                                    </v-flex>
                                </v-layout>
                            </v-container>
                            <v-container>
                                <v-layout>
                                    <v-flex xs12 md6>
                                        <v-text-field
                                          type="time"
                                          v-model="object.beginTime"
                                          label="Время открытия"
                                        ></v-text-field>
                                    </v-flex>
                                    <v-flex xs12 md6>
                                        <v-text-field
                                          type="time"
                                          v-model="object.endTime"
                                          label="Время закрытия"
                                        ></v-text-field>
                                    </v-flex>
                                </v-layout>
                            </v-container>
                            <v-container>
                                <v-layout>
                                    <v-flex xs12 md6>
                                        <v-combobox :items="activityTypes" label="Выберите род деятельности объекта"
                                            v-model="object.activityType" return-object item-value="id" item-text="name" :rules="requiredRule"></v-combobox>
                                    </v-flex>
                                    <v-flex xs12 md6>
                                        <v-text-field name="area" label="Площадь в м², Например: 50" type="text" v-model.trim="object.area" :rules="requiredRule"></v-text-field>
                                    </v-flex>
                                </v-layout>
                            </v-container>
                            <v-container>
                                <v-layout>
                                    <v-flex xs12 md6>
                                        <v-text-field name="renters-count" label="Количество арендаторов" type="text"
                                            v-model.trim="object.rentersCount" :rules="requiredRule"></v-text-field>
                                    </v-flex>
                                    <v-flex xs12 md6>
                                        <v-text-field name="attendance" label="Посещаемость, например 123" type="text" v-model.trim="object.attendance" :rules="requiredRule"></v-text-field>
                                    </v-flex>
                                </v-layout>
                            </v-container>
                            <v-container>
                                <v-layout>
                                    <v-flex xs12 md6>
                                        <v-text-field
                                            name="responsible-person-one-complex-name"
                                            label="Отвественное лицо ФИО 1"
                                            type="text"
                                            v-model.trim="object.responsiblePersonOne.complexName"
                                            :rules="requiredRule"
                                            ></v-text-field>
                                    </v-flex>
                                    <v-flex xs12 md3>
                                        <v-text-field
                                            name="responsible-person-one-complex-phone"
                                            label="Телефон"
                                            type="phone"
                                            v-model.trim="object.responsiblePersonOne.phone"
                                            :rules="requiredRule"
                                            ></v-text-field>
                                    </v-flex>
                                    <v-flex xs12 md3>
                                        <v-text-field
                                            name="responsible-person-one-complex-email"
                                            label="Почта"
                                            type="email"
                                            v-model.trim="object.responsiblePersonOne.email"
                                            :rules="requiredRule"
                                            ></v-text-field>
                                    </v-flex>
                                </v-layout>
                            </v-container>
                            <v-container>
                                <v-layout>
                                    <v-flex xs12 md6>
                                        <v-text-field
                                            name="responsible-person-two-complex-name"
                                            label="Отвественное лицо ФИО 2"
                                            type="text"
                                            v-model.trim="object.responsiblePersonTwo.complexName"
                                            ></v-text-field>
                                    </v-flex>
                                    <v-flex xs12 md3>
                                        <v-text-field
                                            name="responsible-person-two-complex-phone"
                                            label="Телефон"
                                            type="phone"
                                            v-model.trim="object.responsiblePersonTwo.phone"
                                            ></v-text-field>
                                    </v-flex>
                                    <v-flex xs12 md3>
                                        <v-text-field
                                            name="responsible-person-two-complex-email"
                                            label="Почта"
                                            type="email"
                                            v-model.trim="object.responsiblePersonTwo.email"
                                            ></v-text-field>
                                    </v-flex>
                                </v-layout>
                            </v-container>
                            <v-container>
                                <v-layout justify-center align-content-center>
                                    <v-flex offset-5 class="justify-center">
                                        <h3>
                                            Конфигурация генерации плейлистов
                                        </h3>
                                    </v-flex>
                                </v-layout>
                                <v-layout>
                                  <v-flex xs12 md3>
                                    <v-text-field
                                        name="playlist-max-advert-length"
                                        label="Максимальная длина рекламного блока в секундах"
                                        :hint="formatSeconds(object.maxAdvertBlockInSeconds)"
                                        type="number"
                                        v-model="object.maxAdvertBlockInSeconds"
                                    ></v-text-field>
                                  </v-flex>
                                </v-layout>
                            </v-container>
                            <v-container>
                                <v-layout justify-center align-content-center>
                                    <v-flex offset-5 class="justify-center">
                                        <h3>
                                            Нерабочие дни
                                        </h3>
                                    </v-flex>
                                </v-layout>
                                <v-layout>
                                    <v-flex v-for="weekDay in weekDays" :key="weekDay.value">
                                        <v-checkbox :label="weekDay.name" @change="(checked) => freeDayChanged(weekDay.value)"></v-checkbox>
                                    </v-flex>
                                </v-layout>
                            </v-container>
                            <v-container>
                                <v-layout row justify-space-between wrap>
                                    <v-flex xs12 sm12 md12>
                                        <v-text-field label="Фильтр подборок" prepend-icon="search" v-debounce="findSelection">
                                        </v-text-field>
                                    </v-flex>
                                    <v-flex v-for="selection in selections" v-bind:key="selection.id">
                                        <v-checkbox
                                            v-show="selection.isVisible"
                                            v-bind:key="selection.id"
                                            :label="selection.name"
                                            :value="selection.id"
                                            @change="() => selectionChanged(selection.id)">
                                        </v-checkbox>
                                    </v-flex>
                                </v-layout>
                            </v-container>
                        </v-form>
                    </v-card-text>
                    <v-card-actions>
                        <v-spacer></v-spacer>
                        <v-container>
                            <v-layout>
                                <v-flex xs-12 sm-12 md-12>
                                    <v-file-input
                                        :placeholder="objectImage == null ? 'Файл не загружен' : 'Выбран файл'"
                                        accept="image/*"
                                        @change="fileChanged"
                                        :showSize="true"
                                        >
                                    </v-file-input>
                                </v-flex>
                                <v-flex  xs-12 sm-12 md-12>
                                    <v-btn color="primary" @click="addObjectButtonClick">Добавить</v-btn>
                                </v-flex>
                                <v-flex  xs-12 sm-12 md-12>
                                    <v-btn color="primary" @click="cancelButtonClick">Отмена</v-btn>
                                </v-flex>
                            </v-layout>
                        </v-container>
                    </v-card-actions>
                </v-card>
            </v-flex>
        </v-layout>
    </v-container>
</template>

<script>
    import {addSeconds, format} from "date-fns";
    import dayOfWeek from '../helpers/DayOfWeek';
    import userHomePage from "@/helpers/UserHomePage";

    export default {
        name: 'add-object',
        data: () => ({
            beginTimeMenu: false,
            beginTimeModal: false,
            endTimeMenu: false,
            endTimeModal: false,
            object: {
                name: '',
                beginTime: '',
                endTime: '',
                actualAddress: '',
                legalAddress: '',
                activityType: null,
                serviceCompany: null,
                city: null,
                attendance: '',
                area: '',
                rentersCount: '',
                priority: null,
                responsiblePersonOne: {
                    complexName: '',
                    phone: '',
                    email: ''
                },
                responsiblePersonTwo: {
                    complexName: '',
                    phone: '',
                    email: ''
                },
                freeDays: [],
                selections: [],
                maxAdvertBlockInSeconds: 5 * 60,
            },
            weekDays: [],
            activityTypes: [],
            serviceCompanies: [],
            cities: [],
            selections: [],
            objectImage: null,
            requiredRule: [
                v => !!v || 'Поле обязательно для заполнения'
            ],
            priorityRule: [
                v => !!v || 'Поле обязательно для заполнения',
                v => (v > 0 && v < 7) || 'Приоритет должен быть от 1 до 6'
            ]
        }),
        methods: {
            cancelButtonClick(){
              this.$router.push(`/${userHomePage.defaultPage}#objects`);
            },
            findSelection(text) {
                if (text === ''){
                    this.selections.forEach(o => o.isVisible = true);
                    return;
                }
                const wrongSelections = this.selections.filter(o => o.name.toLowerCase().indexOf(text.toLowerCase()) === -1);
                wrongSelections.forEach(o => o.isVisible = false);
                this.selections.filter(o => !wrongSelections.includes(o)).forEach(o => o.isVisible = true);
            },
            selectionChanged(selectionId) {
                const sel = this.selections.find(o => o.id === selectionId)
                const exists = this.object.selections.includes(sel)

                if (!exists) {
                    this.object.selections.push(sel)
                } else {
                    this.object.selections = this.object.selections.filter(o => o !== sel)
                }
            },
            async addObjectButtonClick() {
                if (!this.$refs.form.validate()) {
                    return
                }
                let formData = new FormData()
                formData.append('images', this.objectImage)
                formData.append('objectInfoModel', JSON.stringify(this.object))

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
            },
            freeDayChanged(freeDay){
                if (this.object.freeDays.includes(freeDay)){
                    this.object.freeDays = [... this.object.freeDays.filter(fd => fd != freeDay)];
                    return;
                }
                this.object.freeDays.push(freeDay);
            },
            formatSeconds(seconds) {
              return format(addSeconds(new Date(0), seconds), 'mm:ss');
            },
        },

        async beforeMount() {
            for (const day in dayOfWeek) {
                switch (day) {
                    case 'Monday':
                        this.weekDays.push({name: 'Понедельник', value: day});
                        break;
                    case 'Tuesday':
                        this.weekDays.push({name: 'Вторник', value: day});
                        break;
                    case 'Wednesday':
                        this.weekDays.push({name: 'Среда', value: day});
                        break;
                    case 'Thursday':
                        this.weekDays.push({name: 'Четверг', value: day});
                        break;
                    case 'Friday':
                        this.weekDays.push({name: 'Пятница', value: day});
                        break;
                    case 'Saturday':
                        this.weekDays.push({name: 'Суббота', value: day});
                        break;
                    case 'Sunday':
                        this.weekDays.push({name: 'Воскресенье', value: day});
                        break;
                    default:
                        break;
                }
            }

            const activityTypePromise = this.axios.get('activity-type');
            const serviceCompanyPromise = this.axios.get('service-company');
            const citiesPromise = this.axios.get('cities');
            const selectionsPromise = this.axios.get('selections?actual=true&page=1&itemsPerPage=10000');

            const result = await Promise.all([activityTypePromise, serviceCompanyPromise, citiesPromise, selectionsPromise]);

            result[0].data.forEach(el => {
                this.activityTypes.push(el);
            });

            result[1].data.forEach(el => {
                this.serviceCompanies.push(el);
            });

            result[2].data.forEach(el => {
                this.cities.push(el);
            });

            result[3].data.result.forEach(el => {
                this.selections.push({...el, isVisible: true });
            });
        },
    }
</script>
