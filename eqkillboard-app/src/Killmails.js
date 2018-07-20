import React, { Component } from 'react';
import { Query } from 'react-apollo';
import gql from 'graphql-tag';

export const KillmailDataFragment = gql`
fragment KillmailData on Killmail {
  nodeId
  id,
  characterByVictimId {
    id,
    name
  },
  guildByVictimGuildId {
    id, 
    name
  },
  victimLevel,
  characterByAttackerId {
    id,
    name
  },
  guildByAttackerGuildId {
    id,
    name
  },
  attackerLevel,
  zoneByZoneId {
    id,
    name
  },
  killedAt  
}
`;

class Killmails extends Component {
  render() {
    return (
      this.props.killmails.map(killmail => {
        let victimGuildName = killmail.guildByVictimGuildId ? killmail.guildByVictimGuildId.name : '';
        let victimLevel = killmail.victimLevel ? killmail.victimLevel : 'Unknown level';
        let attackerGuildName = killmail.guildByAttackerGuildId ? killmail.guildByAttackerGuildId.name : '';
        let attackerLevel = killmail.attackerLevel ? killmail.attackerLevel : 'Unknown level';
  
        return (
          <p>
            {killmail.killedAt}: {killmail.characterByVictimId.name} ({victimLevel}) &lt;{victimGuildName}&gt; has been killed by {killmail.characterByAttackerId.name} ({attackerLevel}) &lt;{attackerGuildName}&gt; in {killmail.zoneByZoneId.name}
          </p>
        )
      })
    )

  }
}

export default Killmails;
