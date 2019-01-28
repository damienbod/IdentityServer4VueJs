<template>
    <div class="home">
        <img alt="Vue logo" src="../assets/logo.png">
        <div class="home">
            <p v-if="isLoggedIn">User: {{ username }}</p>
            <button @click="login" v-if="!isLoggedIn">Login</button>
            <button @click="logout" v-if="isLoggedIn">Logout</button>
            <button @click="getProtectedApiData" v-if="isLoggedIn">Get API data</button>
        </div>

        <ul v-if="dataEventRecordsItems && dataEventRecordsItems.length">
            <li v-for="dataEventRecordsItem of dataEventRecordsItems">
                <p>{{dataEventRecordsItem.Id}} {{dataEventRecordsItem.Name}}  {{dataEventRecordsItem.Description}} {{dataEventRecordsItem.Timestamp}}</p>
            </li>
        </ul>

    </div>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import AuthService from '@/services/auth.service';

import axios from 'axios';

const auth = new AuthService();

@Component({
  components: {
  },
})

export default class Home extends Vue {
    public currentUser: string = '';
    public accessTokenExpired: boolean | undefined = false;
    public isLoggedIn: boolean = false;

    public dataEventRecordsItems: [] = [];

    get username(): string {
        return this.currentUser;
    }

    public login() {
        auth.login();
    }

    public logout() {
        auth.logout();
    }

    public mounted() {
        auth.getUser().then((user) => {
            this.currentUser = user.profile.name;
            this.accessTokenExpired = user.expired;

            this.isLoggedIn = (user !== null && !user.expired);
        });
    }

    public getProtectedApiData() {

        auth.getAccessToken().then((userToken: string) => {
            axios.defaults.headers.common['Authorization'] = `Bearer ${userToken}`;

            axios.get('https://localhost:44355/api/DataEventRecords/')
                .then((response: any) => {
                    this.dataEventRecordsItems = response.data;
                })
                .catch((error: any) => {
                    alert(error);
                });
        });
    }
}
</script>
