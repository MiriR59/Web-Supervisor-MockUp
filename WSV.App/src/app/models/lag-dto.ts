export interface LagDto{
    sourceId: number;

    state: LagState;

    latestGenerated?: string;

    latestDb?: string;

    dbLag?: number;
}

export type LagState = 'Ok' | 'NoLiveData' | 'DbEmpty';