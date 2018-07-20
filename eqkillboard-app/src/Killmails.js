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
      <div></div>
    )
  }
}

export default Killmails;
