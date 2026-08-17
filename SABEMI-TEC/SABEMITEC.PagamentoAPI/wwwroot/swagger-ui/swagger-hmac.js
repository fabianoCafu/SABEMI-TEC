//const SECRET = "MINHA_CHAVE_SECRETA_COMPARTILHADA";

//window.onload = () => {

//    const interval = setInterval(() => {

//        if (!window.ui)
//            return;

//        clearInterval(interval);

//        const originalInterceptor = window.ui.getConfigs().requestInterceptor;

//        window.ui.getConfigs().requestInterceptor = async (req) => {

//            const body = typeof req.body === "string" ? req.body : JSON.stringify(req.body ?? "");
//            const encoder = new TextEncoder();
//            const key = await crypto.subtle.importKey("raw", encoder.encode(SECRET), { name: "HMAC", hash: "SHA-256"},false,["sign"]);
//            const signature = await crypto.subtle.sign("HMAC", key, encoder.encode(body));
//            const hex = [...new Uint8Array(signature)].map(b => b.toString(16).padStart(2, "0")).join("").toUpperCase();

//            req.headers["X-Signature"] = hex;

//            console.log("Body:", body);
//            console.log("Signature:", hex);

//            if (originalInterceptor) {
//                return originalInterceptor(req);
//            }
            
//            return req;
//        };

//    }, 100);
//};



requestInterceptor: async (req) => {

    const secret = "MINHA_CHAVE_SECRETA_COMPARTILHADA";
    const body = req.body ?? "";
    const encoder = new TextEncoder();
    const key = await crypto.subtle.importKey("raw", encoder.encode(secret), { name: "HMAC", hash: "SHA-256" }, false, ["sign"]);
    const signature = await crypto.subtle.sign("HMAC", key, encoder.encode(body));
    const hex = Array.from(new Uint8Array(signature)).map(b => b.toString(16).padStart(2, "0")).join("").toUpperCase();

    req.headers["X-Signature"] = hex;

    return req;
}