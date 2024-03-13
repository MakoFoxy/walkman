export function parseJwt (token) {
    let data = null;
    let jwtIsCorrect = true;
    try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));
        data = JSON.parse(jsonPayload);

        let expireDate = new Date(0);
        expireDate.setUTCSeconds(data.exp);

        if (expireDate < new Date()){
            jwtIsCorrect = false;
        }
    } catch (error) {
        jwtIsCorrect = false;
    }

    return {
        token: data,
        tokenIsCorrect: jwtIsCorrect
    };
}
