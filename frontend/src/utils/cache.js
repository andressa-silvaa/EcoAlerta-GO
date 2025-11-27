import { CACHE_TTL_MS } from '../constants/appConfig';

const LOG_PREFIX = '💾 Cache';

class ApiCache {
  constructor(ttl = CACHE_TTL_MS) {
    this.cache = new Map();
    this.ttl = ttl;
  }

  generateKey(endpoint, params = {}) {
    const sortedParams = Object.keys(params)
      .sort()
      .filter((key) => params[key] != null)
      .map((key) => `${key}=${params[key]}`)
      .join('&');
    return sortedParams ? `${endpoint}?${sortedParams}` : endpoint;
  }

  get(key) {
    const cached = this.cache.get(key);
    if (!cached) return null;

    if (this._isExpired(cached.timestamp)) {
      this.cache.delete(key);
      return null;
    }

    console.log(`${LOG_PREFIX} ✨ HIT:`, key);
    return cached.data;
  }

  set(key, data) {
    console.log(`${LOG_PREFIX} SET:`, key);
    this.cache.set(key, {
      data,
      timestamp: Date.now(),
    });
  }

  clear() {
    console.log(`${LOG_PREFIX} 🗑️ Cleared`);
    this.cache.clear();
  }

  size() {
    return this.cache.size;
  }

  _isExpired(timestamp) {
    return Date.now() - timestamp > this.ttl;
  }
}

export const apiCache = new ApiCache();

