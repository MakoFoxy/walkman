<template>
    <div>
        <v-container fluid fill-height>
            <v-layout align-center justify-center>
                <v-flex xs12 sm8 md4>
                    <v-card class="elevation-12">
                        <v-toolbar dark color="primary">
                            <v-toolbar-title>Просмотр рекламы</v-toolbar-title>
                            <v-spacer></v-spacer>
                        </v-toolbar>
                        <v-card-text>
                            <v-form>
                                <v-text-field label="Название рекламы" type="text" v-model="advert.name" readonly></v-text-field>
                                <v-text-field label="Дата начала" type="text" :value="formatDate(advert.fromDate)" readonly></v-text-field>
                                <v-text-field label="Дата окончания" type="text" :value="formatDate(advert.toDate)" readonly></v-text-field>
                                <v-text-field label="Количество выходов" type="text" v-model="advert.repeatCount" readonly></v-text-field>
                                <v-text-field label="Клиент" type="text" v-model="advert.client" readonly></v-text-field>
                                <v-textarea label="Объекты на которых выходит реклама" v-model="advert.objects" readonly></v-textarea>
                            </v-form>
                        </v-card-text>
                        <v-card-actions>
                            <v-spacer></v-spacer>
                            <audio :src="getTrackUrl()" controls></audio>
                            <v-btn color="primary" @click="closeButtonClick">Закрыть</v-btn>
                        </v-card-actions>
                    </v-card>
                </v-flex>
            </v-layout>
        </v-container>
    </div>
</template>

<script>
import moment from 'moment'
import {PublisherUrl, Advert} from '@/helpers/Constants.js'
import userHomePage from "@/helpers/UserHomePage";

    export default {
        name: 'advert-card',
        data: () => ({
            advert: {
                name: '',
                fromDate: '',
                toDate: '',
                trackType: '',
                objects: '',
                repeatCount: '',
                client: ''
            }
        }),
        async beforeMount() {
            const advertResponse = await this.axios.get(`/adverts/${this.$route.query.id}`);
            this.advert = {...advertResponse.data, objects: advertResponse.data.objects.map(o => o.name)};
        },
        methods: {
            formatDate(date){
                if (date === ''){
                    return ''
                }
                return moment(date).format('DD.MM.YYYY')
            },
            getTrackUrl(){
                return `${PublisherUrl}api/v1/track?trackId=${this.$route.query.id}&TrackType=${Advert}`
            },
            closeButtonClick() {
              this.$router.push(`/${userHomePage.defaultPage}#adverts`)
            },
        }
    }
</script>
