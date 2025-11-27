class SustainabilityMetrics {
  constructor() {
    this.metrics = {
      totalApiCalls: 0,
      cachedApiCalls: 0,
      dataTransferred: 0,
      renderCount: 0,
      avgResponseTime: 0,
    };
    this.startTime = performance.now();
  }

  recordApiCall(isCached = false, dataSize = 0) {
    this.metrics.totalApiCalls++;
    if (isCached) this.metrics.cachedApiCalls++;
    this.metrics.dataTransferred += dataSize;
  }

  recordRender() {
    this.metrics.renderCount++;
  }

  getCacheEfficiency() {
    if (this.metrics.totalApiCalls === 0) return 0;
    return ((this.metrics.cachedApiCalls / this.metrics.totalApiCalls) * 100).toFixed(1);
  }

  getEstimatedCO2Saved() {
    const mbTransferred = this.metrics.dataTransferred / (1024 * 1024);
    const savedRequests = this.metrics.cachedApiCalls;
    const avgRequestSize = 0.5;
    const co2PerMB = 0.5;
    
    return (savedRequests * avgRequestSize * co2PerMB).toFixed(2);
  }

  getReport() {
    const uptime = ((performance.now() - this.startTime) / 1000 / 60).toFixed(1);
    
    return {
      uptime: `${uptime} minutos`,
      totalApiCalls: this.metrics.totalApiCalls,
      cacheHits: this.metrics.cachedApiCalls,
      cacheEfficiency: `${this.getCacheEfficiency()}%`,
      dataTransferred: `${(this.metrics.dataTransferred / 1024).toFixed(1)} KB`,
      renderCount: this.metrics.renderCount,
      estimatedCO2Saved: `${this.getEstimatedCO2Saved()}g`,
      sustainabilityScore: this.calculateScore(),
    };
  }

  calculateScore() {
    const cacheEfficiency = parseFloat(this.getCacheEfficiency());
    const renderEfficiency = this.metrics.totalApiCalls > 0 
      ? Math.min(100, (this.metrics.totalApiCalls / this.metrics.renderCount) * 100)
      : 100;
    
    return Math.round((cacheEfficiency * 0.6 + renderEfficiency * 0.4));
  }

  logReport() {
    console.group('🌱 Relatório de Sustentabilidade Digital (ISO 14000)');
    const report = this.getReport();
    Object.entries(report).forEach(([key, value]) => {
      const label = key.replace(/([A-Z])/g, ' $1').replace(/^./, str => str.toUpperCase());
      console.log(`${label}: ${value}`);
    });
    console.groupEnd();
  }
}

export const sustainabilityMetrics = new SustainabilityMetrics();

if (typeof window !== 'undefined') {
  setInterval(() => {
    sustainabilityMetrics.logReport();
  }, 5 * 60 * 1000);
}

