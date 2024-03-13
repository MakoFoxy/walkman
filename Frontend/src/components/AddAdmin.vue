<template>
    <div>
        <v-container fluid fill-height>
            <v-layout align-center justify-center>
                <v-flex xs12 sm12 md12>
                    <v-card class="elevation-12">
                        <v-toolbar dark color="primary">
                            <v-toolbar-title>Добавление администратора</v-toolbar-title>
                            <v-spacer></v-spacer>
                        </v-toolbar>
                        <v-card-text>
                            <v-form v-model="valid" lazy-validation ref="form">
                                <v-text-field :rules="requiredRule" name="firstName" label="Фамилия" type="text"
                                    v-model.trim="manager.firstName"></v-text-field>
                                <v-text-field :rules="requiredRule" name="secondName" label="Имя" type="text"
                                    v-model.trim="manager.secondName"></v-text-field>
                                <v-text-field name="lastName" label="Отчество" type="text"
                                    v-model.trim="manager.lastName"></v-text-field>
                                <v-text-field :rules="requiredRule" name="Email" label="Email" type="email"
                                    v-model.trim="manager.email"></v-text-field>
                                <v-text-field :rules="requiredRule" name="phoneNumber" label="Телефон" type="text"
                                    v-model.trim="manager.phoneNumber"></v-text-field>
                                <v-combobox :items="roles" label="Выберите роль"
                                    v-model="manager.role" return-object item-value="id" item-text="name" :rules="requiredRule"></v-combobox>
                                <v-combobox :items="objects" label="Выберите объекты"
                                    v-model="manager.objects" return-object item-value="id" item-text="name" multiple></v-combobox>
                                <v-text-field :rules="requiredRule" name="password" label="Пароль" type="password"
                                    v-model.trim="manager.password"></v-text-field>
                            </v-form>
                        </v-card-text>
                        <v-card-actions>
                            <v-spacer></v-spacer>
                            <v-btn color="primary" @click="addAdminButtonClick" :disabled="loadingInProgress">Добавить</v-btn>
                            <v-btn color="primary" @click="$router.push('/home#admins')">Отмена</v-btn>
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
    export default {
        name: 'add-admin',
        data: () => ({
            valid: true,
            requiredRule: [
                v => !!v || 'Поле обязательно для заполнения'
            ],
            loadingInProgress: false,
            manager: {
                firstName: '',
                secondName: '',
                lastName: '',
                email: '',
                phoneNumber: '',
                role: null,
                password: '',
                objects: []
            },
            roles: [],
            objects: []
        }),
        async beforeMount() {
            const roleResponse = await this.axios.get(`roles?filter=Admin`);
            this.roles = roleResponse.data;
            const objectsResponse = await this.axios.get(`object/all`);
            this.objects = objectsResponse.data;
        },
        methods: {
            async addAdminButtonClick() {
                if (!this.$refs.form.validate()) {
                    return
                }

                this.loadingInProgress = true

                try {
                    await this.axios.post('managers', {manager: this.manager})
                } catch (err) {
                    this.loadingInProgress = false
                    return
                }

                this.$router.push('/home#admins')
            }
        }
    }
</script>