import { UserManager, WebStorageStateStore, User } from "oidc-client";

export default class AuthService {
    private userManager: UserManager;

    constructor() {
        const STS_DOMAIN: string = "https://localhost:44356"; 

        const settings: any = {
            userStore: new WebStorageStateStore({ store: window.localStorage }),
            authority: STS_DOMAIN,
            client_id: "vuejs_code_client",
            redirect_uri: "https://localhost:44357/callback.html",
            response_type: "code",
            scope: "openid profile dataEventRecords",
            post_logout_redirect_uri: "https://localhost:44357/",
            filterProtocolClaims: true,
            //metadata: {
            //    issuer: STS_DOMAIN + "/",
            //    authorization_endpoint: STS_DOMAIN + "/authorize",
            //    userinfo_endpoint: STS_DOMAIN + "/userinfo",
            //    end_session_endpoint: STS_DOMAIN + "/v2/logout",
            //    jwks_uri: STS_DOMAIN + "/.well-known/jwks.json",
            //}
        };

        this.userManager = new UserManager(settings);
    }

    public getUser(): Promise<User> {
        return this.userManager.getUser();
    }

    public login(): Promise<void> {
        return this.userManager.signinRedirect();
    }

    public logout(): Promise<void> {
        return this.userManager.signoutRedirect();
    }
}