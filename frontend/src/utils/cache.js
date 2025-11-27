/**
 * Sistema de cache simples para requisições de API
 * Reduz chamadas duplicadas ao backend
 */

class ApiCache {
  constructor(ttl = 5 * 60 * 1000) { // 5 minutos padrão
    this.cache = new Map();
    this.ttl = ttl;
  }

  generateKey(endpoint, params) {
    const sortedParams = Object.keys(params || {})
      .sort()
      .map(key => `${key}=${params[key]}`)
      .join('&');
    return `${endpoint}?${sortedParams}`;
  }

  get(key) {
    const cached = this.cache.get(key);
    if (!cached) return null;

    const now = Date.now();
    if (now - cached.timestamp > this.ttl) {
      this.cache.delete(key);
      return null;
    }

    console.log('✨ Cache HIT:', key);
    return cached.data;
  }

  set(key, data) {
    console.log('💾 Cache SET:', key);
    this.cache.set(key, {
      data,
      timestamp: Date.now()
    });
  }

  clear() {
    console.log('🗑️ Cache limpo');
    this.cache.clear();
  }

  size() {
    return this.cache.size;
  }
}

export const apiCache = new ApiCache();

