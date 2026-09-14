import type { AutoPublishSettingResponse } from "../api/generated/fitSyncApi.schemas";
import type { SportCategory } from "./sportCategory";

export function isAutoPublishEnabled(
  settings: AutoPublishSettingResponse[],
  serviceType: string,
  category: SportCategory,
): boolean {
  return settings.some(
    (setting) =>
      setting.serviceType === serviceType && setting.sportCategory === category,
  );
}

export function withAutoPublishCategories(
  settings: AutoPublishSettingResponse[],
  serviceType: string,
  categories: SportCategory[],
): AutoPublishSettingResponse[] {
  return [
    ...settings.filter((setting) => setting.serviceType !== serviceType),
    ...categories.map((sportCategory) => ({ serviceType, sportCategory })),
  ];
}

export function toAutoPublishRequest(settings: AutoPublishSettingResponse[]) {
  return settings.map((setting) => ({
    serviceType: setting.serviceType,
    sportCategory: setting.sportCategory,
  }));
}
