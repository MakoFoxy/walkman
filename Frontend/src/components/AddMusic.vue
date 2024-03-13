<template>
    <div>
        <v-container fluid fill-height>
            <v-layout align-center justify-center>
                <v-flex xs12 sm8 md4>
                    <v-card class="elevation-12">
                        <v-toolbar dark color="primary">
                            <v-toolbar-title>Добавление музыкальных треков</v-toolbar-title>
                            <v-spacer></v-spacer>
                        </v-toolbar>
                        <v-card-text>
                            <v-combobox :items="genres" return-object item-value="id" item-text="name" v-model="selectedGenre" label="Жанры"></v-combobox>

                            <ul>
                                <li v-for="(item, i) in musicFileNames" :key="item">
                                    <div>{{++i}} {{item}} <span style="float: right;cursor: pointer;" @click="removeMusicFile(item)">X</span></div>
                                </li>
                            </ul>
                        </v-card-text>
                        <v-card-actions>
                            <v-spacer></v-spacer>
                            <v-file-input
                                :placeholder="musicFiles.length == 0 ? 'Файлы не загружены' : 'Выбран файл'"
                                accept="audio/*"
                                @change="fileChanged"
                                :showSize="true"
                                :multiple="true"
                                >
                            </v-file-input>
                            <v-btn color="primary" @click="addMusicButtonClick" :disabled="loadingInProgress">Добавить</v-btn>
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
    </div>
</template>

<script>
    import userHomePage from "@/helpers/UserHomePage";

    export default {
        name: 'add-music',
        data: () => ({
            loadingInProgress: false,
            musicFiles: [],
            genres: [],
            selectedGenre: null
        }),
        computed: {
            musicFileNames: {
                get(){
                    return this.musicFiles.map(el => el.name)
                }
            }
        },
        async mounted(){
            const response = await this.axios.get('genres?page=1&itemsPerPage=10000')
            this.genres = response.data
        },
        methods: {
            async addMusicButtonClick() {
                if (this.musicFiles.length === 0) {
                    return
                }

                if (typeof this.selectedGenre === 'string'){
                    alert('Выберите жанр из выпадающего списка, для создания нового жанра обратитесь к администратору');
                    return;
                }

                let formData = new FormData()
                formData.append('genre', JSON.stringify(this.selectedGenre))
                this.musicFiles.forEach(el => {
                    formData.append('musicFiles[]', el)
                })
                this.loadingInProgress = true

                try {
                    await this.axios.post('music', formData, {
                        headers: {
                            'Content-Type': 'multipart/form-data'
                        }
                    })
                } catch (err) {
                    this.loadingInProgress = false
                    return
                }

              this.$router.push(`/${userHomePage.defaultPage}#music`)
            },
            removeMusicFile(musicFile){
                this.musicFiles = this.musicFiles.filter(el => el.name !== musicFile)
            },
            fileChanged(files) {
                this.musicFiles = Array.from(files)
            },
            cancelButtonClick() {
              this.$router.push(`/${userHomePage.defaultPage}#music`)
            }
        }
    }
</script>
