| Komponente | Wie prüfen? | Folge bei Ausfall |
|---|---|---|
| ApiGateway (Nginx) | Browser/GET http://localhost:8000 | Client erreicht keine APIs/Services |
| MessageQueue (RabbitMQ) | Web-UI http://localhost:8001 + AMQP 5672 | Events/Commands werden nicht verteilt |
| ArticleService | GET /health | Produktdaten nicht verfügbar |
| AuthService | GET /health + Login-Test | Login/Token nicht möglich |
| ClientService | GET /health | Kundenverwaltung nicht möglich |
| OrderService | GET /health | Bestellprozess steht |
| EventLogService | GET /health | Ereignisnachvollziehbarkeit fehlt |


**Hinweis:** In verteilten Systemen ist ein schneller Health-Check pro Komponente zentral, weil Fehler oft nur Teilbereiche betreffen. Die Tabelle zeigt, welche Komponente mit welchem Signal geprüft wird und welche Auswirkungen ein Ausfall hat.
