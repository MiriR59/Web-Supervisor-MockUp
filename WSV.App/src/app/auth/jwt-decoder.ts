export function decodeJwtPayload(token: string): any | null {
    try {
        const parts = token.split('.');
        if (parts.length !== 3)
            return null;

        const base64Url = parts[1];

        let base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');

        const padding = base64.length % 4;
        if (padding !== 0) {
            base64 += '='.repeat(4 - padding);
        }

        //Decode
        const json = atob(base64);
        return JSON.parse(json);
    } catch {
        return null;
    }
}

const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';

const NAME_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name';

export function getRole(payload: any): string | null {
    return payload?.[ROLE_CLAIM] ?? null;
}

export function getUserName(payload: any): string | null {
    return payload?.[NAME_CLAIM] ?? null;
}

export function isExpired(payload: any, clockSkewSeconds = 30): boolean {
    const exp = payload?.exp;
    if (!exp)
        return true;

    const nowSeconds = Math.floor(Date.now() / 1000);
    return exp <= nowSeconds + clockSkewSeconds;
}